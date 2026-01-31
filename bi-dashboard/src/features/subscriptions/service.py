"""
Subscriptions Service
Lógica de negócio para assinaturas
"""

from decimal import Decimal
from .repository import SubscriptionsRepository
from .schemas import SubscriptionSummaryDTO


class SubscriptionsService:
    """Service para análise de assinaturas"""
    
    def __init__(self, repository: SubscriptionsRepository):
        self.repository = repository
    
    def get_subscription_summary(self) -> SubscriptionSummaryDTO:
        """Resumo com cálculo de churn"""
        raw = self.repository.get_subscriptions_summary()
        
        total = raw.get('TotalSubscriptions', 0)
        active = raw.get('ActiveSubscriptions', 0)
        mrr = Decimal(str(raw.get('MonthlyRecurringRevenue', 0)))
        
        churn_rate = 0.0
        if total > 0:
            churn_rate = ((total - active) / total) * 100
        
        return SubscriptionSummaryDTO(
            TotalSubscriptions=total,
            ActiveSubscriptions=active,
            MonthlyRecurringRevenue=mrr,
            ChurnRate=round(churn_rate, 2)
        )


def create_subscriptions_service() -> SubscriptionsService:
    """Factory para SubscriptionsService"""
    return SubscriptionsService(SubscriptionsRepository())
