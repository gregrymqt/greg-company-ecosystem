"""
Financial Analytics DTOs
Data Transfer Objects para análises financeiras
Baseado em ALL_MODELS.txt
"""

from dataclasses import dataclass
from datetime import datetime
from typing import Optional
from decimal import Decimal


@dataclass
class PaymentDTO:
    """
    DTO para Payments
    Campos baseados em ALL_MODELS.txt - Payments
    """
    Id: str
    ExternalId: str
    UserId: str
    Status: str
    PayerEmail: str
    Method: str
    Installments: int
    DateApproved: Optional[datetime]
    LastFourDigits: str
    CustomerCpf: str
    Amount: Decimal
    Description: str
    SubscriptionId: str
    CreatedAt: datetime
    UpdatedAt: Optional[datetime]
    UserName: Optional[str] = None
    UserEmail: Optional[str] = None


@dataclass
class PaymentSummaryDTO:
    """
    DTO para resumo financeiro de pagamentos
    """
    TotalPayments: int
    TotalApproved: Decimal
    TotalPending: Decimal
    TotalCancelled: Decimal
    UniqueCustomers: int
    AvgTicket: Decimal
    ApprovalRate: float
    

@dataclass
class ChargebackDTO:
    """
    DTO para Chargeback
    Campos baseados em ALL_MODELS.txt - Chargeback
    """
    Id: int
    ChargebackId: int
    PaymentId: int
    UserId: Optional[str]
    Status: str  # Novo, AguardandoEvidencias, EvidenciasEnviadas, Ganhamos, Perdemos
    Amount: Decimal
    CreatedAt: datetime
    InternalNotes: Optional[str]
    UserName: Optional[str] = None
    UserEmail: Optional[str] = None


@dataclass
class ChargebackSummaryDTO:
    """
    DTO para resumo de chargebacks
    """
    TotalChargebacks: int
    TotalAmount: Decimal
    Novo: int
    AguardandoEvidencias: int
    Ganhamos: int
    Perdemos: int
    WinRate: float


@dataclass
class RevenueMetricsDTO:
    """
    DTO para métricas de receita agregadas
    """
    TotalRevenue: Decimal
    MonthlyRevenue: Decimal
    YearlyRevenue: Decimal
    TotalTransactions: int
    AverageTransactionValue: Decimal
    TopPaymentMethod: str
    PaymentMethodDistribution: dict
