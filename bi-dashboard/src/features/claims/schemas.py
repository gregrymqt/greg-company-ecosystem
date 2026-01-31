"""
Claims Analytics DTOs (Schemas)
Data Transfer Objects para análises de risco e disputas
Baseado em ALL_MODELS.txt - Claims
"""

from dataclasses import dataclass
from datetime import datetime
from typing import Optional, List
from decimal import Decimal


@dataclass
class ClaimAnalyticsDTO:
    """
    DTO para Analytics de Claims (usado pela API)
    Campos essenciais para inteligência de risco
    """
    id: int
    mp_claim_id: int  # MpClaimId do Mercado Pago
    amount_at_risk: Decimal  # Valor do pagamento em disputa
    claim_type: str  # ClaimType Enum
    internal_status: str  # InternalClaimStatus Enum
    days_open: int  # Dias desde criação
    panel_url: str  # URL dinâmica do painel MP
    
    # Campos adicionais opcionais
    resource_id: Optional[str] = None
    current_stage: Optional[str] = None
    user_name: Optional[str] = None
    user_email: Optional[str] = None
    payment_status: Optional[str] = None
    date_created: Optional[datetime] = None
    
    @property
    def is_critical(self) -> bool:
        """Identifica se a claim é crítica (> 30 dias aberta)"""
        return self.days_open > 30
    
    @property
    def is_resolved(self) -> bool:
        """Verifica se a claim foi resolvida"""
        return self.internal_status in ['ResolvidoGanhamos', 'ResolvidoPerdemos']
    
    @property
    def we_won(self) -> bool:
        """Verifica se ganhamos a disputa"""
        return self.internal_status == 'ResolvidoGanhamos'


@dataclass
class ClaimDTO:
    """
    DTO para Claims (disputas/chargebacks)
    Campos baseados em ALL_MODELS.txt - Claims
    """
    Id: str
    PaymentId: str
    ExternalId: str
    Status: str  # opened, accepted, rejected, cancelled
    Type: str  # chargeback, dispute, claim
    Amount: Decimal
    Reason: str
    DateCreated: datetime
    DateResolved: Optional[datetime]
    
    # Campos do Payment relacionado (JOIN)
    PaymentStatus: Optional[str] = None
    PayerEmail: Optional[str] = None
    UserId: Optional[str] = None
    UserName: Optional[str] = None
    
    @property
    def IsUnresolved(self) -> bool:
        """Verifica se a disputa ainda está aberta"""
        return self.Status in ['opened', 'in_process']
    
    @property
    def ResolutionDays(self) -> Optional[int]:
        """Calcula dias para resolução (se resolvido)"""
        if self.DateResolved:
            delta = self.DateResolved - self.DateCreated
            return delta.days
        return None


@dataclass
class ClaimSummaryDTO:
    """
    DTO para resumo de disputas/chargebacks
    """
    TotalClaims: int
    OpenedClaims: int
    AcceptedClaims: int
    RejectedClaims: int
    CancelledClaims: int
    TotalAmountAtRisk: Decimal
    AverageResolutionDays: Optional[float]
    DisputeRate: float  # % de pagamentos em disputa
    WinRate: float  # % de disputas rejeitadas (vencidas pelo merchant)


@dataclass
class ClaimByReasonDTO:
    """
    DTO para disputas agrupadas por motivo
    """
    Reason: str
    Count: int
    TotalAmount: Decimal
    Percentage: float


@dataclass
class ClaimTrendDTO:
    """
    DTO para tendência temporal de disputas
    """
    Period: str  # ex: "2024-01", "Week 23"
    ClaimsOpened: int
    ClaimsResolved: int
    AmountAtRisk: Decimal
    WinRate: float
