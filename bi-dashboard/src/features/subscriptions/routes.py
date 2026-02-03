"""
Subscriptions Routes
Endpoints para gestão e analytics de assinaturas
"""
import logging
from fastapi import APIRouter, Depends, BackgroundTasks, HTTPException, status

# Imports da Feature
from src.features.subscriptions.service import create_subscriptions_service, SubscriptionsService
from src.features.subscriptions.schemas import SubscriptionSummaryDTO
from src.core.security import get_current_user

logger = logging.getLogger(__name__)

# Router protegido
router = APIRouter()

# ==============================================================================
# NOVO ENDPOINT: SYNC ROWS
# ==============================================================================

@router.post("/sync-rows", status_code=status.HTTP_202_ACCEPTED)
async def sync_subscriptions_rows(
    background_tasks: BackgroundTasks,
    service: SubscriptionsService = Depends(create_subscriptions_service)
):
    """
    Sincroniza KPIs de Assinaturas (MRR, Churn) e Lista Recente com o Rows.com.
    """
    try:
        background_tasks.add_task(service.sync_subscriptions_to_rows)
        return {
            "message": "Sincronização de Assinaturas iniciada em background.",
            "targets": ["KPIs", "Detailed List"]
        }
    except Exception as e:
        logger.error(f"Erro ao iniciar sync de subscriptions: {str(e)}")
        raise HTTPException(status_code=500, detail="Falha ao iniciar sincronização")

# ==============================================================================
# ENDPOINTS DE LEITURA (KPIs)
# ==============================================================================

@router.get("/summary")
async def get_subscription_summary(
    service: SubscriptionsService = Depends(create_subscriptions_service)
):
    """
    Retorna o resumo de assinaturas (MRR, Churn, Totais).
    """
    try:
        # Retorna o DTO diretamente
        return service.get_subscription_summary()
    except Exception as e:
        logger.error(f"Erro ao buscar resumo de assinaturas: {str(e)}")
        raise HTTPException(status_code=500, detail="Erro interno ao buscar resumo")