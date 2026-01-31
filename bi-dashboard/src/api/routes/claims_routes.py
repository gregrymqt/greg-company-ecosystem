"""
Claims REST Routes
"""

from fastapi import APIRouter, HTTPException
from typing import List
from ...features.claims import create_claims_service, ClaimAnalyticsDTO
from ...features.claims.websocket_handlers import (
    broadcast_kpi_update,
    broadcast_new_claim
)

router = APIRouter()


@router.get("/kpis")
async def get_claims_kpis():
    """Retorna KPIs consolidados de claims"""
    try:
        service = create_claims_service()
        kpis = service.get_claims_kpis()
        
        # Broadcast via WebSocket
        await broadcast_kpi_update(kpis)
        
        return kpis
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.get("/active")
async def get_active_claims():
    """Retorna claims ativas"""
    try:
        service = create_claims_service()
        claims = service.get_active_claims_analytics()
        
        return {
            "total": len(claims),
            "claims": [
                {
                    "id": claim.id,
                    "mpClaimId": claim.mp_claim_id,
                    "amountAtRisk": float(claim.amount_at_risk),
                    "claimType": claim.claim_type,
                    "status": claim.internal_status,
                    "daysOpen": claim.days_open,
                    "isCritical": claim.is_critical,
                    "panelUrl": claim.panel_url
                }
                for claim in claims
            ]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.get("/critical")
async def get_critical_claims():
    """Retorna claims críticas (> 30 dias)"""
    try:
        service = create_claims_service()
        claims = service.get_critical_claims()
        
        return {
            "total": len(claims),
            "claims": [
                {
                    "id": claim.id,
                    "mpClaimId": claim.mp_claim_id,
                    "amountAtRisk": float(claim.amount_at_risk),
                    "daysOpen": claim.days_open,
                    "panelUrl": claim.panel_url
                }
                for claim in claims
            ]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.get("/revenue-at-risk")
async def get_revenue_at_risk():
    """Retorna faturamento em risco"""
    try:
        service = create_claims_service()
        revenue = service.calculate_revenue_at_risk()
        
        return {
            "revenueAtRisk": float(revenue)
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
