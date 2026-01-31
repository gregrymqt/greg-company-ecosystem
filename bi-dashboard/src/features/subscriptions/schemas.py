"""
Subscriptions Feature - Schemas
DTOs para análise de assinaturas
"""

from dataclasses import dataclass
from datetime import datetime
from typing import Optional
from decimal import Decimal


@dataclass
class SubscriptionDTO:
    """DTO para Subscription"""
    Id: str
    UserId: str
    Status: str
    CurrentAmount: int
    CurrentPeriodEndDate: datetime
    PlanName: Optional[str] = None


@dataclass
class SubscriptionSummaryDTO:
    """DTO para resumo de assinaturas"""
    TotalSubscriptions: int
    ActiveSubscriptions: int
    MonthlyRecurringRevenue: Decimal
    ChurnRate: float
