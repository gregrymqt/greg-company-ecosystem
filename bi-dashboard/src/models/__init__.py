"""
Models Package
Exporta todos os DTOs (Data Transfer Objects)
"""

from .product_dto import ProductDTO
from .cleaned_product_dto import CleanedProductDTO
from .financial_dto import (
    PaymentDTO,
    PaymentSummaryDTO,
    ChargebackDTO,
    ChargebackSummaryDTO,
    RevenueMetricsDTO
)
from .subscription_dto import (
    SubscriptionDTO,
    SubscriptionSummaryDTO,
    PlanDTO,
    PlanMetricsDTO,
    SubscriptionExpirationDTO,
    MRRAnalysisDTO
)
from .support_dto import (
    TicketDTO,
    TicketSummaryDTO,
    TicketByContextDTO,
    TicketTrendDTO,
    UserSupportMetricsDTO
)

__all__ = [
    # Product DTOs
    'ProductDTO',
    'CleanedProductDTO',
    # Financial DTOs
    'PaymentDTO',
    'PaymentSummaryDTO',
    'ChargebackDTO',
    'ChargebackSummaryDTO',
    'RevenueMetricsDTO',
    # Subscription DTOs
    'SubscriptionDTO',
    'SubscriptionSummaryDTO',
    'PlanDTO',
    'PlanMetricsDTO',
    'SubscriptionExpirationDTO',
    'MRRAnalysisDTO',
    # Support DTOs
    'TicketDTO',
    'TicketSummaryDTO',
    'TicketByContextDTO',
    'TicketTrendDTO',
    'UserSupportMetricsDTO',
]