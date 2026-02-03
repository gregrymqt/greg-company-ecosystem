import asyncio
from typing import Dict, Any, List
from decimal import Decimal
from datetime import datetime, timedelta

# Infra Greg Company
from ...core.infrastructure.rows_client import rows_client
from ...features.rows.service import rows_service
from ...core.infrastructure.redis_client import get_or_create_async

from .repository import FinancialRepository
from .schemas import PaymentSummaryDTO, RevenueMetricsDTO, ChargebackSummaryDTO

class FinancialService:
    def __init__(self, repository: FinancialRepository):
        self.repository = repository

    # ==================== PAYMENTS & REVENUE (CACHED) ====================

    async def get_payment_summary(self, use_cache: bool = True) -> PaymentSummaryDTO:
        cache_key = "financial:summary:payments"

        async def _calculate():
            raw = await self.repository.get_payments_summary()
            
            # Lógica de negócio
            total = raw.get('TotalPayments', 0)
            approved = await self.repository.get_payments_by_status('approved')
            approval_rate = (len(approved) / total * 100) if total > 0 else 0.0

            return PaymentSummaryDTO(
                TotalPayments=total,
                TotalApproved=Decimal(str(raw.get('TotalApproved', 0) or 0)),
                TotalPending=Decimal(str(raw.get('TotalPending', 0) or 0)),
                TotalCancelled=Decimal(str(raw.get('TotalCancelled', 0) or 0)),
                UniqueCustomers=raw.get('UniqueCustomers', 0),
                AvgTicket=Decimal(str(raw.get('AvgTicket', 0) or 0)),
                ApprovalRate=round(approval_rate, 2)
            )

        if use_cache:
            return await get_or_create_async(cache_key, _calculate, ttl_seconds=120)
        return await _calculate()

    async def get_revenue_metrics(self, use_cache: bool = True) -> RevenueMetricsDTO:
        cache_key = "financial:metrics:revenue"

        async def _calculate():
            # Busca TODOS os pagamentos aprovados (pode ser pesado, por isso o cache é vital)
            approved_payments = await self.repository.get_payments_by_status('approved')
            
            if not approved_payments:
                return RevenueMetricsDTO.empty() # Supondo que você crie um método estático helper

            # Processamento em memória (CPU bound, não bloqueia IO)
            total_rev = sum(Decimal(str(p['Amount'])) for p in approved_payments)
            count = len(approved_payments)
            
            # Filtros de data
            now = datetime.now()
            month_ago = now - timedelta(days=30)
            year_ago = now - timedelta(days=365)
            
            monthly_rev = sum(Decimal(str(p['Amount'])) for p in approved_payments if p['CreatedAt'] >= month_ago)
            yearly_rev = sum(Decimal(str(p['Amount'])) for p in approved_payments if p['CreatedAt'] >= year_ago)
            
            # Distribuição
            methods = {}
            for p in approved_payments:
                m = p.get('Method', 'Unknown')
                methods[m] = methods.get(m, 0) + 1
            top_method = max(methods, key=methods.get) if methods else 'N/A'

            return RevenueMetricsDTO(
                TotalRevenue=total_rev,
                MonthlyRevenue=monthly_rev,
                YearlyRevenue=yearly_rev,
                TotalTransactions=count,
                AverageTransactionValue=(total_rev / count) if count else Decimal(0),
                TopPaymentMethod=top_method,
                PaymentMethodDistribution=methods
            )

        if use_cache:
            return await get_or_create_async(cache_key, _calculate, ttl_seconds=300) # Cache de 5 min
        return await _calculate()

    # ==================== CHARGEBACKS (CACHED) ====================

    async def get_chargeback_summary(self, use_cache: bool = True) -> ChargebackSummaryDTO:
        cache_key = "financial:summary:chargeback"

        async def _calculate():
            raw = await self.repository.get_chargeback_summary()
            
            ganhamos = raw.get('Ganhamos', 0)
            perdemos = raw.get('Perdemos', 0)
            resolved = ganhamos + perdemos
            win_rate = (ganhamos / resolved * 100) if resolved > 0 else 0.0

            return ChargebackSummaryDTO(
                TotalChargebacks=raw.get('TotalChargebacks', 0),
                TotalAmount=Decimal(str(raw.get('TotalAmount', 0) or 0)),
                Novo=raw.get('Novo', 0),
                AguardandoEvidencias=raw.get('AguardandoEvidencias', 0),
                Ganhamos=ganhamos,
                Perdemos=perdemos,
                WinRate=round(win_rate, 2)
            )

        if use_cache:
            return await get_or_create_async(cache_key, _calculate, ttl_seconds=120)
        return await _calculate()

    # ==================== ROWS SYNC (PARALLEL) ====================

    async def sync_financial_to_rows(self):
        # Dispara as 3 consultas ao mesmo tempo (Scatter)
        # Force cache bypass (False) para garantir dados frescos no relatório
        tasks = [
            self.get_payment_summary(use_cache=False),
            self.get_revenue_metrics(use_cache=False),
            self.get_chargeback_summary(use_cache=False)
        ]
        
        # Espera todas terminarem (Gather)
        pay_summary, rev_metrics, cb_summary = await asyncio.gather(*tasks)

        # Monta payloads
        payload_pay = rows_service.build_financial_kpis(pay_summary)
        payload_rev = rows_service.build_revenue_metrics(rev_metrics)
        payload_cb = rows_service.build_chargeback_kpis(cb_summary)

        # Envia para o Rows em paralelo
        await asyncio.gather(
            rows_client.send_data("Financeiro!A1", payload_pay),
            rows_client.send_data("Receita!A1", payload_rev),
            rows_client.send_data("Risco_Chargeback!A1", payload_cb)
        )

        return {"status": "success", "message": "Financial ecosystem synced to Rows."}

def create_financial_service() -> FinancialService:
    return FinancialService(FinancialRepository())