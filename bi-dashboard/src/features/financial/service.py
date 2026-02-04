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
            # Busca APENAS o sumário (1 linha), sem trazer listas pesadas
            raw = await self.repository.get_payments_summary_optimized()
            
            total = raw.get('TotalPayments', 0)
            # CORREÇÃO CRÍTICA: Usamos o count do SQL, não len() de uma lista
            count_approved = raw.get('CountApproved', 0) 
            
            approval_rate = (count_approved / total * 100) if total > 0 else 0.0

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
            # 🚀 SCATTER: Dispara as duas queries ao mesmo tempo
            task_metrics = self.repository.get_revenue_dashboard_metrics()
            task_methods = self.repository.get_payment_method_stats()
            
            # GATHER: Espera ambas terminarem
            raw_metrics, raw_methods = await asyncio.gather(task_metrics, task_methods)
            
            # Processamento Metrics
            total_rev = Decimal(str(raw_metrics.get('TotalRevenue', 0) or 0))
            count = raw_metrics.get('TotalTransactions', 0)

            # Processamento Methods (Top & Distribution)
            # Se a lista vier vazia, fallback seguro
            top_method_name = "N/A"
            dist_dict = {}

            if raw_methods:
                # O primeiro da lista é o Top (pois o SQL ordenou DESC)
                top_method_name = raw_methods[0]['Method']
                
                # Monta o dicionário de distribuição para o gráfico
                for item in raw_methods:
                    method = item['Method'] or "Unknown"
                    amount = Decimal(str(item['TotalAmount'] or 0))
                    dist_dict[method] = amount

            return RevenueMetricsDTO(
                TotalRevenue=total_rev,
                MonthlyRevenue=Decimal(str(raw_metrics.get('MonthlyRevenue', 0) or 0)),
                YearlyRevenue=Decimal(str(raw_metrics.get('YearlyRevenue', 0) or 0)),
                TotalTransactions=count,
                AverageTransactionValue=(total_rev / count) if count else Decimal(0),
                TopPaymentMethod=top_method_name, 
                PaymentMethodDistribution=dist_dict
            )

        if use_cache:
            return await get_or_create_async(cache_key, _calculate, ttl_seconds=300)
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