"""
Features Module
Vertical Slices organizadas por domínio de negócio
Seguindo o padrão do backend C# e frontend React
"""

from .claims import (
    ClaimsRepository,
    ClaimsService,
    create_claims_service,
    ClaimAnalyticsDTO,
)

from .financial import (
    FinancialRepository,
    FinancialService,
    create_financial_service,
    PaymentSummaryDTO,
    RevenueMetricsDTO,
)

__all__ = [
    # Claims
    'ClaimsRepository',
    'ClaimsService',
    'create_claims_service',
    'ClaimAnalyticsDTO',
    
    # Financial
    'FinancialRepository',
    'FinancialService',
    'create_financial_service',
    'PaymentSummaryDTO',
    'RevenueMetricsDTO',
]
