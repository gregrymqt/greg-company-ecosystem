from fastapi import APIRouter, HTTPException, BackgroundTasks, Depends, status
from src.features.support.service import create_support_service, SupportService

router = APIRouter()

# ==============================================================================
# NOVO ENDPOINT: SYNC ROWS
# ==============================================================================

@router.post("/sync-rows", status_code=status.HTTP_202_ACCEPTED)
async def sync_support_rows(
    background_tasks: BackgroundTasks,
    service: SupportService = Depends(create_support_service)
):
    """
    Dispara atualização da dashboard de Suporte no Rows.
    """
    try:
        background_tasks.add_task(service.sync_support_to_rows)
        return {"message": "Sincronização de Suporte iniciada."}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

# ==============================================================================
# ENDPOINTS EXISTENTES (Só para referência)
# ==============================================================================

@router.get("/summary")
async def get_support_summary(
    service: SupportService = Depends(create_support_service)
):
    return service.get_ticket_summary()