"""
Claims Feature Module
Vertical Slice para análise de risco e disputas
"""

from .repository import ClaimsRepository
from .service import ClaimsService, create_claims_service
from .schemas import (
    ClaimAnalyticsDTO,
    ClaimDTO,
    ClaimSummaryDTO,
    ClaimByReasonDTO,
    ClaimTrendDTO,
)
from .enums import (
    InternalClaimStatus,
    ClaimType,
    ClaimStage,
    ClaimResource,
    MpClaimStatus,
)
from .websocket_handlers import setup_claims_hub_handlers

__all__ = [
    # Repository
    'ClaimsRepository',
    
    # Service
    'ClaimsService',
    'create_claims_service',
    
    # Schemas (DTOs)
    'ClaimAnalyticsDTO',
    'ClaimDTO',
    'ClaimSummaryDTO',
    'ClaimByReasonDTO',
    'ClaimTrendDTO',
    
    # Enums
    'InternalClaimStatus',
    'ClaimType',
    'ClaimStage',
    'ClaimResource',
    'MpClaimStatus',
    
    # WebSocket Handlers
    'setup_claims_hub_handlers',
]
