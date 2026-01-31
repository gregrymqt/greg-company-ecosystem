"""
Financial Service - Domain Layer
Serviço de análise financeira seguindo DDD
Transformações e lógica de negócio para Payments e Chargebacks
"""

from typing import List, Dict, Any
from datetime import datetime, timedelta
from decimal import Decimal
from ..data.repositories.financial_repository import FinancialRepository
from ..models.financial_dto import (
    PaymentDTO,
    PaymentSummaryDTO,
    ChargebackDTO,
    ChargebackSummaryDTO,
    RevenueMetricsDTO
)


class FinancialService:
    """
    Serviço de domínio para análises financeiras
    Recebe FinancialRepository via injeção de dependência
    """
    
    def __init__(self, financial_repository: FinancialRepository):
        """
        Inicializa o serviço com o repositório
        
        Args:
            financial_repository: Instância do FinancialRepository
        """
        self.repository = financial_repository
    
    # ==================== PAYMENTS ANALYTICS ====================
    
    def get_payment_summary(self) -> PaymentSummaryDTO:
        """
        Retorna resumo financeiro de pagamentos com cálculos adicionais
        
        Returns:
            PaymentSummaryDTO com métricas calculadas
        """
        raw_summary = self.repository.get_payments_summary()
        
        # Conversões de tipos
        total_payments = raw_summary.get('TotalPayments', 0)
        total_approved = Decimal(str(raw_summary.get('TotalApproved', 0)))
        total_pending = Decimal(str(raw_summary.get('TotalPending', 0)))
        total_cancelled = Decimal(str(raw_summary.get('TotalCancelled', 0)))
        unique_customers = raw_summary.get('UniqueCustomers', 0)
        avg_ticket = Decimal(str(raw_summary.get('AvgTicket', 0)))
        
        # Cálculo da taxa de aprovação
        approval_rate = 0.0
        if total_payments > 0:
            # Conta quantos foram aprovados
            approved_count = self.repository.get_payments_by_status('approved')
            approval_rate = (len(approved_count) / total_payments) * 100
        
        return PaymentSummaryDTO(
            TotalPayments=total_payments,
            TotalApproved=total_approved,
            TotalPending=total_pending,
            TotalCancelled=total_cancelled,
            UniqueCustomers=unique_customers,
            AvgTicket=avg_ticket,
            ApprovalRate=round(approval_rate, 2)
        )
    
    def get_payments_by_period_analysis(
        self, 
        start_date: datetime, 
        end_date: datetime
    ) -> Dict[str, Any]:
        """
        Análise detalhada de pagamentos em um período
        
        Args:
            start_date: Data inicial
            end_date: Data final
            
        Returns:
            Dicionário com análises do período
        """
        df = self.repository.get_payments_by_period(start_date, end_date)
        
        if df.empty:
            return {
                "period": {"start": start_date, "end": end_date},
                "total_payments": 0,
                "total_revenue": Decimal('0'),
                "daily_average": Decimal('0'),
                "top_method": None
            }
        
        # Filtra apenas aprovados
        approved_df = df[df['Status'] == 'approved']
        
        # Cálculos
        total_payments = len(approved_df)
        total_revenue = Decimal(str(approved_df['Amount'].sum()))
        
        # Média diária
        days_diff = (end_date - start_date).days or 1
        daily_average = total_revenue / days_diff
        
        # Método de pagamento mais usado
        top_method = None
        if not approved_df.empty:
            top_method = approved_df['Method'].mode().iloc[0] if len(approved_df['Method'].mode()) > 0 else None
        
        return {
            "period": {
                "start": start_date.isoformat(),
                "end": end_date.isoformat()
            },
            "total_payments": total_payments,
            "total_revenue": float(total_revenue),
            "daily_average": float(daily_average),
            "top_method": top_method,
            "payment_methods": approved_df['Method'].value_counts().to_dict() if not approved_df.empty else {}
        }
    
    def get_revenue_metrics(self) -> RevenueMetricsDTO:
        """
        Calcula métricas agregadas de receita
        
        Returns:
            RevenueMetricsDTO com métricas calculadas
        """
        # Todos os pagamentos aprovados
        approved_payments = self.repository.get_payments_by_status('approved')
        
        if not approved_payments:
            return RevenueMetricsDTO(
                TotalRevenue=Decimal('0'),
                MonthlyRevenue=Decimal('0'),
                YearlyRevenue=Decimal('0'),
                TotalTransactions=0,
                AverageTransactionValue=Decimal('0'),
                TopPaymentMethod='N/A',
                PaymentMethodDistribution={}
            )
        
        # Total geral
        total_revenue = sum(Decimal(str(p['Amount'])) for p in approved_payments)
        total_transactions = len(approved_payments)
        avg_transaction = total_revenue / total_transactions if total_transactions > 0 else Decimal('0')
        
        # Receita mensal (últimos 30 dias)
        thirty_days_ago = datetime.utcnow() - timedelta(days=30)
        monthly_payments = [
            p for p in approved_payments 
            if p.get('CreatedAt') and p['CreatedAt'] >= thirty_days_ago
        ]
        monthly_revenue = sum(Decimal(str(p['Amount'])) for p in monthly_payments)
        
        # Receita anual (últimos 365 dias)
        one_year_ago = datetime.utcnow() - timedelta(days=365)
        yearly_payments = [
            p for p in approved_payments 
            if p.get('CreatedAt') and p['CreatedAt'] >= one_year_ago
        ]
        yearly_revenue = sum(Decimal(str(p['Amount'])) for p in yearly_payments)
        
        # Distribuição por método de pagamento
        payment_methods = {}
        for payment in approved_payments:
            method = payment.get('Method', 'Unknown')
            payment_methods[method] = payment_methods.get(method, 0) + 1
        
        # Método mais usado
        top_method = max(payment_methods, key=payment_methods.get) if payment_methods else 'N/A'
        
        return RevenueMetricsDTO(
            TotalRevenue=total_revenue,
            MonthlyRevenue=monthly_revenue,
            YearlyRevenue=yearly_revenue,
            TotalTransactions=total_transactions,
            AverageTransactionValue=avg_transaction,
            TopPaymentMethod=top_method,
            PaymentMethodDistribution=payment_methods
        )
    
    # ==================== CHARGEBACK ANALYTICS ====================
    
    def get_chargeback_summary(self) -> ChargebackSummaryDTO:
        """
        Retorna resumo de chargebacks com cálculos de taxa de vitória
        Usa campos corretos de ALL_MODELS.txt
        
        Returns:
            ChargebackSummaryDTO com métricas calculadas
        """
        raw_summary = self.repository.get_chargeback_summary()
        
        # Conversões de tipos
        total_chargebacks = raw_summary.get('TotalChargebacks', 0)
        total_amount = Decimal(str(raw_summary.get('TotalAmount', 0)))
        novo = raw_summary.get('Novo', 0)
        aguardando = raw_summary.get('AguardandoEvidencias', 0)
        ganhamos = raw_summary.get('Ganhamos', 0)
        perdemos = raw_summary.get('Perdemos', 0)
        
        # Cálculo da taxa de vitória (Win Rate)
        win_rate = 0.0
        resolved_total = ganhamos + perdemos
        if resolved_total > 0:
            win_rate = (ganhamos / resolved_total) * 100
        
        return ChargebackSummaryDTO(
            TotalChargebacks=total_chargebacks,
            TotalAmount=total_amount,
            Novo=novo,
            AguardandoEvidencias=aguardando,
            Ganhamos=ganhamos,
            Perdemos=perdemos,
            WinRate=round(win_rate, 2)
        )
    
    def get_chargebacks_analysis(self) -> Dict[str, Any]:
        """
        Análise detalhada de chargebacks
        
        Returns:
            Dicionário com análises de chargebacks
        """
        all_chargebacks = self.repository.get_all_chargebacks()
        summary = self.get_chargeback_summary()
        
        # Conversão para DTOs
        chargeback_dtos = [
            ChargebackDTO(
                Id=cb['Id'],
                ChargebackId=cb['ChargebackId'],
                PaymentId=cb['PaymentId'],
                UserId=cb.get('UserId'),
                Status=self._map_chargeback_status(cb['Status']),
                Amount=Decimal(str(cb['Amount'])),
                CreatedAt=cb['CreatedAt'],
                InternalNotes=cb.get('InternalNotes'),
                UserName=cb.get('UserName'),
                UserEmail=cb.get('UserEmail')
            )
            for cb in all_chargebacks
        ]
        
        return {
            "summary": {
                "total": summary.TotalChargebacks,
                "total_amount": float(summary.TotalAmount),
                "win_rate": summary.WinRate
            },
            "by_status": {
                "novo": summary.Novo,
                "aguardando_evidencias": summary.AguardandoEvidencias,
                "ganhamos": summary.Ganhamos,
                "perdemos": summary.Perdemos
            },
            "chargebacks": [
                {
                    "id": cb.Id,
                    "amount": float(cb.Amount),
                    "status": cb.Status,
                    "user": cb.UserName or cb.UserEmail
                }
                for cb in chargeback_dtos[:10]  # Top 10
            ]
        }
    
    # ==================== UTILITY METHODS ====================
    
    def _map_chargeback_status(self, status_code: int) -> str:
        """
        Mapeia código numérico do status para string legível
        Baseado em ALL_MODELS.txt - ChargebackStatus enum
        """
        status_map = {
            0: "Novo",
            1: "AguardandoEvidencias",
            2: "EvidenciasEnviadas",
            3: "Ganhamos",
            4: "Perdemos"
        }
        return status_map.get(status_code, "Desconhecido")
