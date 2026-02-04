from dataclasses import dataclass
from typing import Optional
from decimal import Decimal
from datetime import datetime

@dataclass
class SubscriptionDTO:
    """DTO para Subscription (Lista Detalhada)"""
    Id: str
    UserId: str
    Status: str
    CurrentAmount: Decimal
    CurrentPeriodEndDate: datetime
    PlanName: Optional[str] = None
    # Faltavam estes campos que o JOIN do Repository traz:
    UserName: Optional[str] = "N/A"
    UserEmail: Optional[str] = "N/A"

@dataclass
class SubscriptionSummaryDTO:
    """DTO para resumo de assinaturas (KPIs)"""
    TotalSubscriptions: int
    ActiveSubscriptions: int
    # Faltavam estes campos calculados no SQL:
    PausedSubscriptions: int
    CancelledSubscriptions: int
    MonthlyRecurringRevenue: Decimal
    ChurnRate: float