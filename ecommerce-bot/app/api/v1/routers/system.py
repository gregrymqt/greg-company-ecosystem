import logging
from typing import List
from fastapi import APIRouter, HTTPException, Depends
from pydantic import BaseModel

from app.services.system_service import SystemService
from app.dependencies.rate_limiter import check_demo_rate_limit

logger = logging.getLogger(__name__)
router = APIRouter(tags=["System Operations"])

class DemoRequest(BaseModel):
    urls: List[str]

@router.post("/demo", dependencies=[Depends(check_demo_rate_limit)])
async def request_demo(payload: DemoRequest):
    if len(payload.urls) > 3:
        raise HTTPException(status_code=400, detail="Máximo de 3 URLs permitidas para a demo.")
        
    try:
        await SystemService.process_demo_request(payload.urls)
        return {"status": "enviado_para_fila"}
    except Exception as e:
        logger.error(f"Erro ao publicar demo na fila: {e}")
        raise HTTPException(status_code=500, detail="Erro interno ao processar a solicitação")

@router.get("/export")
async def export_data(tenant_id: str, platform: str = "shopify"):
    try:
        await SystemService.process_export(tenant_id, platform)
        return {"status": "success", "message": f"Exportação concluída para a plataforma {platform}. Arquivo gerado no servidor."}
    except Exception as e:
        logger.error(f"Erro ao exportar dados: {e}")
        raise HTTPException(status_code=500, detail="Erro interno durante a exportação")

@router.get("/health")
async def health_check():
    return {"status": "ok"}
