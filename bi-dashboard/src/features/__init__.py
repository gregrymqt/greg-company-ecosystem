"""
Features Module
Vertical Slices organizadas por domínio de negócio
Seguindo o padrão do backend C# e frontend React
"""

# Claims
from .claims import (
    ClaimsRepository,
    ClaimsService,
    create_claims_service,
    ClaimSummaryDTO,
    ClaimAnalyticsDTO,
)

# Financial
from .financial import (
    FinancialRepository,
    FinancialService,
    create_financial_service,
    PaymentSummaryDTO,
    RevenueMetricsDTO,
    ChargebackSummaryDTO,
)

# Subscriptions
from .subscriptions import (
    SubscriptionsRepository,
    SubscriptionsService,
    create_subscriptions_service,
    SubscriptionSummaryDTO,
    SubscriptionDTO,
)

# Support
from .support import (
    SupportRepository,
    SupportService,
    create_support_service,
    TicketSummaryDTO,
    TicketDTO,
)

# Storage
from .storage import (
    StorageRepository,
    StorageService,
    create_storage_service,
    StorageStatsDTO,
    FileDetailDTO,
)

# Users
from .users import (
    UsersRepository,
    UsersService,
    create_users_service,
    UserSummaryDTO,
    UserDTO,
)

# Content
from .content import (
    ContentRepository,
    ContentService,
    create_content_service,
    ContentSummaryDTO,
    CourseDTO,
)

# Rows (Integration Service)
from .rows import (
    rows_service,
    RowsUpdateRequest,
    RowsCellValue,
)

__all__ = [
    # Claims
    'ClaimsRepository',
    'ClaimsService',
    'create_claims_service',
    'ClaimSummaryDTO',
    'ClaimAnalyticsDTO',
    
    # Financial
    'FinancialRepository',
    'FinancialService',
    'create_financial_service',
    'PaymentSummaryDTO',
    'RevenueMetricsDTO',
    'ChargebackSummaryDTO',

    # Subscriptions
    'SubscriptionsRepository',
    'SubscriptionsService',
    'create_subscriptions_service',
    'SubscriptionSummaryDTO',
    'SubscriptionDTO',

    # Support
    'SupportRepository',
    'SupportService',
    'create_support_service',
    'TicketSummaryDTO',
    'TicketDTO',

    # Storage
    'StorageRepository',
    'StorageService',
    'create_storage_service',
    'StorageStatsDTO',
    'FileDetailDTO',

    # Users
    'UsersRepository',
    'UsersService',
    'create_users_service',
    'UserSummaryDTO',
    'UserDTO',

    # Content
    'ContentRepository',
    'ContentService',
    'create_content_service',
    'ContentSummaryDTO',
    'CourseDTO',

    # Rows
    'rows_service',
    'RowsUpdateRequest',
    'RowsCellValue',
]
