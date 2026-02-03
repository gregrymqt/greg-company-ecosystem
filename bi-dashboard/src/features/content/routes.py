"""
Content Routes
Endpoints para analytics de Cursos e Vídeos
"""
import logging
from fastapi import APIRouter, Depends, BackgroundTasks, HTTPException, status, Query
from typing import List

from src.features.content.service import create_content_service, ContentService
from src.features.content.schemas import ContentSummaryDTO, CourseDTO

logger = logging.getLogger(__name__)

# Router sem proteção explícita (tratada no main.py)
router = APIRouter()

# ==============================================================================
# SYNC ROWS
# ==============================================================================

@router.post("/sync-rows", status_code=status.HTTP_202_ACCEPTED)
async def sync_content_rows(
    background_tasks: BackgroundTasks,
    service: ContentService = Depends(create_content_service)
):
    """
    Sincroniza KPIs de Conteúdo e Lista de Cursos com o Rows em background.
    """
    try:
        background_tasks.add_task(service.sync_content_to_rows)
        return {"message": "Sincronização de Conteúdo iniciada."}
    except Exception as e:
        logger.error(f"Erro no sync de content: {str(e)}")
        raise HTTPException(status_code=500, detail="Erro ao iniciar sync")

# ==============================================================================
# LEITURA
# ==============================================================================

@router.get("/summary", response_model=ContentSummaryDTO)
async def get_content_summary(
    service: ContentService = Depends(create_content_service)
):
    """Retorna KPIs calculados de cursos e vídeos"""
    return service.get_content_summary()

@router.get("/courses", response_model=List[CourseDTO])
async def get_courses_list(
    limit: int = Query(50, ge=1, le=500),
    service: ContentService = Depends(create_content_service)
):
    """Retorna lista de cursos com contagem de vídeos"""
    return service.get_courses_list(limit)