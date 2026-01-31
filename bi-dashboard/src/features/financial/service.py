"""
Financial Service
Service layer para inteligência financeira
Lógica de negócio para Payments e Chargebacks
"""

from typing import List, Dict, Any
from datetime import datetime, timedelta
from decimal import Decimal
from .repository import FinancialRepository
from .schemas import (
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


# ==================== FACTORY ====================

def create_financial_service() -> FinancialService:
    """
    Factory method para criar instância do FinancialService com dependências
    
    Returns:
        FinancialService configurado
    """
    financial_repository = FinancialRepository()
    return FinancialService(financial_repository)
