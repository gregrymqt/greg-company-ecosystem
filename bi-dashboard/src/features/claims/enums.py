"""
Claims Enums
Enumerações para Claims baseadas em ClaimsEnums.cs
Mapeamento dos Enums do C# para Python
"""

from enum import Enum


class InternalClaimStatus(str, Enum):
    """
    Status interno de acompanhamento de Claims
    Baseado em Models/Claims.cs - InternalClaimStatus
    """
    NOVO = "Novo"
    EM_ANALISE = "EmAnalise"
    RESPONDIDO_PELO_VENDEDOR = "RespondidoPeloVendedor"
    RESOLVIDO_GANHAMOS = "ResolvidoGanhamos"
    RESOLVIDO_PERDEMOS = "ResolvidoPerdemos"


class ClaimType(str, Enum):
    """
    Tipos de Claims do Mercado Pago
    Baseado em Models/Enums/ClaimsEnums.cs - ClaimType
    """
    MEDIATIONS = "Mediations"
    CANCEL_PURCHASE = "CancelPurchase"
    RETURN = "Return"
    CANCEL_SALE = "CancelSale"
    CHANGE = "Change"


class ClaimStage(str, Enum):
    """
    Estágios de Claims do Mercado Pago
    Baseado em Models/Enums/ClaimsEnums.cs - ClaimStage
    """
    CLAIM = "Claim"
    DISPUTE = "Dispute"
    RECONTACT = "Recontact"
    NONE = "None"


class ClaimResource(str, Enum):
    """
    Tipos de recursos associados ao Claim
    Baseado em Models/Enums/ClaimsEnums.cs - ClaimResource
    """
    PAYMENT = "Payment"
    ORDER = "Order"
    SHIPMENT = "Shipment"
    PURCHASE = "Purchase"


class MpClaimStatus(str, Enum):
    """
    Status original do Mercado Pago para Claims
    Baseado em Models/Enums/ClaimsEnums.cs - MpClaimStatus
    """
    OPENED = "Opened"
    CLOSED = "Closed"
