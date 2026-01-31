"""
Financial Feature Module
Vertical Slice para análise financeira (Payments, Subscriptions, Chargebacks)
"""

from .repository import FinancialRepository
from .service import FinancialService, create_financial_service
from .schemas import (
    PaymentDTO,
    PaymentSummaryDTO,
    ChargebackDTO,
    ChargebackSummaryDTO,
    RevenueMetricsDTO,
)

__all__ = [
    # Repository
    'FinancialRepository',
    
    # Service
    'FinancialService',
    'create_financial_service',
    
    # Schemas (DTOs)
    'PaymentDTO',
    'PaymentSummaryDTO',
    'ChargebackDTO',
    'ChargebackSummaryDTO',
    'RevenueMetricsDTO',
]
