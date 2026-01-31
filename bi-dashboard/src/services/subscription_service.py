"""
Subscription Service - Domain Layer
Serviço de análise de assinaturas seguindo DDD
Transformações e lógica de negócio para Subscriptions e Plans
"""

from typing import List, Dict, Any
from datetime import datetime, timedelta
from decimal import Decimal
from ..data.repositories.financial_repository import FinancialRepository
from ..models.subscription_dto import (
    SubscriptionDTO,
    SubscriptionSummaryDTO,
    PlanDTO,
    PlanMetricsDTO,
    SubscriptionExpirationDTO,
    MRRAnalysisDTO
)


class SubscriptionService:
    """
    Serviço de domínio para análises de assinaturas
    Recebe FinancialRepository via injeção de dependência
    """
    
    def __init__(self, financial_repository: FinancialRepository):
        """
        Inicializa o serviço com o repositório
        
        Args:
            financial_repository: Instância do FinancialRepository
        """
        self.repository = financial_repository
    
    # ==================== SUBSCRIPTION ANALYTICS ====================
    
    def get_subscription_summary(self) -> SubscriptionSummaryDTO:
        """
        Retorna resumo de assinaturas com cálculos de churn e retenção
        Usa campos corretos de ALL_MODELS.txt
        
        Returns:
            SubscriptionSummaryDTO com métricas calculadas
        """
        raw_summary = self.repository.get_subscriptions_summary()
        
        # Conversões de tipos
        total_subs = raw_summary.get('TotalSubscriptions', 0)
        active_subs = raw_summary.get('ActiveSubscriptions', 0)
        cancelled_subs = raw_summary.get('CancelledSubscriptions', 0)
        paused_subs = raw_summary.get('PausedSubscriptions', 0)
        mrr = Decimal(str(raw_summary.get('MonthlyRecurringRevenue', 0)))
        
        # Cálculo de Churn Rate (taxa de cancelamento)
        churn_rate = 0.0
        if total_subs > 0:
            churn_rate = (cancelled_subs / total_subs) * 100
        
        # Cálculo de Retention Rate (taxa de retenção)
        retention_rate = 0.0
        if total_subs > 0:
            retention_rate = ((total_subs - cancelled_subs) / total_subs) * 100
        
        return SubscriptionSummaryDTO(
            TotalSubscriptions=total_subs,
            ActiveSubscriptions=active_subs,
            CancelledSubscriptions=cancelled_subs,
            PausedSubscriptions=paused_subs,
            MonthlyRecurringRevenue=mrr,
            ChurnRate=round(churn_rate, 2),
            RetentionRate=round(retention_rate, 2)
        )
    
    def get_active_subscriptions_details(self) -> List[SubscriptionDTO]:
        """
        Retorna lista detalhada de assinaturas ativas
        
        Returns:
            Lista de SubscriptionDTO
        """
        active_subs = self.repository.get_active_subscriptions()
        
        return [
            SubscriptionDTO(
                Id=sub['Id'],
                ExternalId=sub.get('ExternalId', ''),
                UserId=sub['UserId'],
                Status=sub['Status'],
                PlanId=sub.get('PlanId', 0),
                CurrentAmount=sub['CurrentAmount'],
                CurrentPeriodStartDate=sub['CurrentPeriodStartDate'],
                CurrentPeriodEndDate=sub['CurrentPeriodEndDate'],
                LastFourCardDigits=sub.get('LastFourCardDigits', ''),
                PayerEmail=sub['PayerEmail'],
                PaymentMethodId=sub.get('PaymentMethodId', ''),
                CreatedAt=sub.get('CreatedAt', datetime.utcnow()),
                UpdatedAt=sub.get('UpdatedAt'),
                PlanName=sub.get('PlanName'),
                PlanAmount=Decimal(str(sub['PlanAmount'])) if sub.get('PlanAmount') else None,
                UserName=sub.get('UserName'),
                UserEmail=sub.get('UserEmail')
            )
            for sub in active_subs
        ]
    
    def get_expiring_subscriptions(self, days: int = 7) -> List[SubscriptionExpirationDTO]:
        """
        Retorna assinaturas que expiram nos próximos X dias
        
        Args:
            days: Número de dias para considerar (padrão: 7)
            
        Returns:
            Lista de SubscriptionExpirationDTO
        """
        expiring = self.repository.get_subscriptions_expiring_soon(days)
        now = datetime.utcnow()
        
        return [
            SubscriptionExpirationDTO(
                Id=sub['Id'],
                UserId=sub['UserId'],
                UserName=sub.get('UserName', 'N/A'),
                UserEmail=sub.get('UserEmail', 'N/A'),
                PlanName=sub.get('PlanName', 'N/A'),
                CurrentPeriodEndDate=sub['CurrentPeriodEndDate'],
                CurrentAmount=sub['CurrentAmount'],
                DaysUntilExpiration=(sub['CurrentPeriodEndDate'] - now).days
            )
            for sub in expiring
        ]
    
    def get_mrr_analysis(self) -> MRRAnalysisDTO:
        """
        Análise detalhada de MRR (Monthly Recurring Revenue)
        Calcula crescimento, expansão e contração
        
        Returns:
            MRRAnalysisDTO com análise de MRR
        """
        summary = self.get_subscription_summary()
        current_mrr = summary.MonthlyRecurringRevenue
        
        # Para cálculo completo de MRR, seria necessário histórico
        # Por enquanto, retorna valores básicos
        # Em produção, você buscaria dados históricos do banco
        
        new_mrr = Decimal('0')  # MRR de novos clientes no mês
        expansion_mrr = Decimal('0')  # MRR de upgrades
        contraction_mrr = Decimal('0')  # MRR de downgrades
        churned_mrr = Decimal('0')  # MRR perdido por cancelamentos
        
        # Net MRR Growth = New + Expansion - Contraction - Churned
        net_mrr_growth = new_mrr + expansion_mrr - contraction_mrr - churned_mrr
        
        # Growth Rate
        growth_rate = 0.0
        if current_mrr > Decimal('0'):
            growth_rate = float((net_mrr_growth / current_mrr) * 100)
        
        return MRRAnalysisDTO(
            CurrentMRR=current_mrr,
            NewMRR=new_mrr,
            ExpansionMRR=expansion_mrr,
            ContractionMRR=contraction_mrr,
            ChurnedMRR=churned_mrr,
            NetMRRGrowth=net_mrr_growth,
            GrowthRate=round(growth_rate, 2)
        )
    
    # ==================== PLAN ANALYTICS ====================
    
    def get_all_plans(self) -> List[PlanDTO]:
        """
        Retorna todos os planos disponíveis
        Usa campos corretos de ALL_MODELS.txt
        
        Returns:
            Lista de PlanDTO
        """
        plans = self.repository.get_all_plans()
        
        return [
            PlanDTO(
                Id=plan['Id'],
                PublicId=str(plan['PublicId']),
                ExternalPlanId=plan['ExternalPlanId'],
                Name=plan['Name'],
                Description=plan.get('Description'),
                TransactionAmount=Decimal(str(plan['TransactionAmount'])),
                CurrencyId=plan['CurrencyId'],
                FrequencyInterval=plan['FrequencyInterval'],
                FrequencyType=self._map_frequency_type(plan['FrequencyType']),
                IsActive=plan['IsActive']
            )
            for plan in plans
        ]
    
    def get_plan_metrics(self) -> List[PlanMetricsDTO]:
        """
        Retorna métricas por plano (assinantes, receita, market share)
        
        Returns:
            Lista de PlanMetricsDTO
        """
        plan_metrics = self.repository.get_plan_metrics()
        
        # Calcula receita total para market share
        total_revenue = sum(
            Decimal(str(pm.get('TotalRevenue', 0))) 
            for pm in plan_metrics
        )
        
        metrics_dtos = []
        for pm in plan_metrics:
            plan_revenue = Decimal(str(pm.get('TotalRevenue', 0)))
            market_share = 0.0
            
            if total_revenue > Decimal('0'):
                market_share = float((plan_revenue / total_revenue) * 100)
            
            metrics_dtos.append(
                PlanMetricsDTO(
                    PlanName=pm['PlanName'],
                    TransactionAmount=Decimal(str(pm['TransactionAmount'])),
                    TotalSubscriptions=pm['TotalSubscriptions'],
                    ActiveSubscriptions=pm['ActiveSubscriptions'],
                    TotalRevenue=plan_revenue,
                    MarketShare=round(market_share, 2)
                )
            )
        
        return metrics_dtos
    
    def get_subscription_health_report(self) -> Dict[str, Any]:
        """
        Relatório consolidado de saúde das assinaturas
        
        Returns:
            Dicionário com análise completa
        """
        summary = self.get_subscription_summary()
        expiring_7d = self.get_expiring_subscriptions(7)
        expiring_30d = self.get_expiring_subscriptions(30)
        mrr_analysis = self.get_mrr_analysis()
        plan_metrics = self.get_plan_metrics()
        
        return {
            "overview": {
                "total_subscriptions": summary.TotalSubscriptions,
                "active": summary.ActiveSubscriptions,
                "mrr": float(summary.MonthlyRecurringRevenue),
                "churn_rate": summary.ChurnRate,
                "retention_rate": summary.RetentionRate
            },
            "expiration_alerts": {
                "expiring_7_days": len(expiring_7d),
                "expiring_30_days": len(expiring_30d)
            },
            "mrr_analysis": {
                "current_mrr": float(mrr_analysis.CurrentMRR),
                "net_growth": float(mrr_analysis.NetMRRGrowth),
                "growth_rate": mrr_analysis.GrowthRate
            },
            "top_plans": [
                {
                    "name": pm.PlanName,
                    "subscribers": pm.ActiveSubscriptions,
                    "revenue": float(pm.TotalRevenue),
                    "market_share": pm.MarketShare
                }
                for pm in sorted(plan_metrics, key=lambda x: x.TotalRevenue, reverse=True)[:3]
            ]
        }
    
    # ==================== UTILITY METHODS ====================
    
    def _map_frequency_type(self, frequency_code: int) -> str:
        """
        Mapeia código de frequência para string
        Baseado em ALL_MODELS.txt - PlanFrequencyType enum
        """
        frequency_map = {
            0: "Days",
            1: "Months"
        }
        return frequency_map.get(frequency_code, "Unknown")
