"""
Subscription Analytics DTOs
Data Transfer Objects para análises de assinaturas
Baseado em ALL_MODELS.txt
"""

from dataclasses import dataclass
from datetime import datetime
from typing import Optional
from decimal import Decimal


@dataclass
class SubscriptionDTO:
    """
    DTO para Subscription
    Campos baseados em ALL_MODELS.txt - Subscription
    """
    Id: str
    ExternalId: str
    UserId: str
    Status: str
    PlanId: int
    CurrentAmount: int
    CurrentPeriodStartDate: datetime
    CurrentPeriodEndDate: datetime
    LastFourCardDigits: str
    PayerEmail: str
    PaymentMethodId: str
    CreatedAt: datetime
    UpdatedAt: Optional[datetime]
    PlanName: Optional[str] = None
    PlanAmount: Optional[Decimal] = None
    UserName: Optional[str] = None
    UserEmail: Optional[str] = None


@dataclass
class SubscriptionSummaryDTO:
    """
    DTO para resumo de assinaturas
    """
    TotalSubscriptions: int
    ActiveSubscriptions: int
    CancelledSubscriptions: int
    PausedSubscriptions: int
    MonthlyRecurringRevenue: Decimal
    ChurnRate: float
    RetentionRate: float


@dataclass
class PlanDTO:
    """
    DTO para Plan
    Campos baseados em ALL_MODELS.txt - Plan
    """
    Id: int
    PublicId: str
    ExternalPlanId: str
    Name: str
    Description: Optional[str]
    TransactionAmount: Decimal
    CurrencyId: str
    FrequencyInterval: int
    FrequencyType: str  # Days, Months
    IsActive: bool


@dataclass
class PlanMetricsDTO:
    """
    DTO para métricas por plano
    """
    PlanName: str
    TransactionAmount: Decimal
    TotalSubscriptions: int
    ActiveSubscriptions: int
    TotalRevenue: Decimal
    MarketShare: float


@dataclass
class SubscriptionExpirationDTO:
    """
    DTO para assinaturas que expiram em breve
    """
    Id: str
    UserId: str
    UserName: str
    UserEmail: str
    PlanName: str
    CurrentPeriodEndDate: datetime
    CurrentAmount: int
    DaysUntilExpiration: int


@dataclass
class MRRAnalysisDTO:
    """
    DTO para análise de MRR (Monthly Recurring Revenue)
    """
    CurrentMRR: Decimal
    NewMRR: Decimal
    ExpansionMRR: Decimal
    ContractionMRR: Decimal
    ChurnedMRR: Decimal
    NetMRRGrowth: Decimal
    GrowthRate: float
