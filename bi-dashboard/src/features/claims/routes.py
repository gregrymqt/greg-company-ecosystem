from fastapi import APIRouter, HTTPException, BackgroundTasks, Depends, status
from typing import List, Dict, Any

# Imports da Feature Claims
# Ajuste os caminhos relativos (...) conforme sua estrutura de pastas real
from src.features.claims.service import create_claims_service, ClaimsService
from src.features.claims.schemas import ClaimAnalyticsDTO
from src.features.claims.handlers import broadcast_kpi_update

router = APIRouter()

# ==============================================================================
# NOVO ENDPOINT: SINCRONIZAÇÃO COM ROWS
# ==============================================================================

@router.post("/sync-rows", status_code=status.HTTP_202_ACCEPTED)
async def sync_claims_to_rows(
    background_tasks: BackgroundTasks,
    service: ClaimsService = Depends(create_claims_service)
):
    """
    Dispara a sincronização dos dados de Claims para o Rows.com.
    Executa em background para não travar a UI.
    """
    try:
        # Adiciona a tarefa na fila de background do FastAPI
        # O método service.sync_claims_to_rows foi o que criamos no passo anterior
        background_tasks.add_task(service.sync_claims_to_rows)
        
        return {
            "message": "Sincronização com Rows iniciada em background.",
            "target": "Claims Dashboard"
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao iniciar sync: {str(e)}")

# ==============================================================================
# ENDPOINTS DE LEITURA (GET) - Refatorados com Depends
# ==============================================================================

@router.get("/kpis")
async def get_claims_kpis(
    service: ClaimsService = Depends(create_claims_service)
):
    """Retorna KPIs consolidados de claims"""
    try:
        kpis = service.get_claims_kpis()
        
        # Opcional: Broadcast via WebSocket a cada refresh manual
        await broadcast_kpi_update(kpis)
        
        return kpis
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.get("/active")
async def get_active_claims(
    service: ClaimsService = Depends(create_claims_service)
):
    """Retorna claims ativas"""
    try:
        claims = service.get_active_claims_analytics()
        
        # Retorna formatado para o Frontend
        return {
            "total": len(claims),
            "claims": [
                {
                    "id": c.id,
                    "mpClaimId": c.mp_claim_id,
                    "amountAtRisk": float(c.amount_at_risk),
                    "claimType": c.claim_type,
                    "status": c.internal_status,
                    "daysOpen": c.days_open,
                    "isCritical": c.is_critical, # Usa a propriedade calculada no DTO
                    "panelUrl": c.panel_url
                }
                for c in claims
            ]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.get("/critical")
async def get_critical_claims(
    service: ClaimsService = Depends(create_claims_service)
):
    """Retorna claims críticas (> 30 dias)"""
    try:
        claims = service.get_critical_claims()
        
        return {
            "total": len(claims),
            "claims": [
                {
                    "id": c.id,
                    "mpClaimId": c.mp_claim_id,
                    "amountAtRisk": float(c.amount_at_risk),
                    "daysOpen": c.days_open,
                    "panelUrl": c.panel_url
                }
                for c in claims
            ]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.get("/revenue-at-risk")
async def get_revenue_at_risk(
    service: ClaimsService = Depends(create_claims_service)
):
    """Retorna faturamento em risco"""
    try:
        revenue = service.calculate_revenue_at_risk()
        
        return {
            "revenueAtRisk": float(revenue)
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))