"""
Users Routes
Endpoints para analytics de usuários
"""
import logging
from fastapi import APIRouter, Depends, BackgroundTasks, HTTPException, status, Query

from src.features.users.service import create_users_service, UsersService
from src.features.users.schemas import UserSummaryDTO

logger = logging.getLogger(__name__)

# Router SEM proteção explícita (já tratada no main.py)
router = APIRouter()

# ==============================================================================
# SYNC ROWS
# ==============================================================================

@router.post("/sync-rows", status_code=status.HTTP_202_ACCEPTED)
async def sync_users_rows(
    background_tasks: BackgroundTasks,
    service: UsersService = Depends(create_users_service)
):
    """
    Sincroniza dados de usuários com o Rows em background.
    """
    try:
        background_tasks.add_task(service.sync_users_to_rows)
        return {"message": "Sincronização de Usuários iniciada."}
    except Exception as e:
        logger.error(f"Erro no sync de users: {str(e)}")
        raise HTTPException(status_code=500, detail="Erro ao iniciar sync")

# ==============================================================================
# LEITURA
# ==============================================================================

@router.get("/summary", response_model=UserSummaryDTO)
async def get_users_summary(
    service: UsersService = Depends(create_users_service)
):
    """Retorna KPIs de usuários"""
    return service.get_user_summary()

@router.get("/list")
async def get_users_list(
    limit: int = Query(50, ge=1, le=500),
    service: UsersService = Depends(create_users_service)
):
    """Retorna lista de usuários"""
    return service.get_users_list(limit)