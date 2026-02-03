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
from .websocket_handlers import setup_financial_hub_handlers

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
    
    # WebSocket Handlers
    'setup_financial_hub_handlers',
]
