import logging
from typing import List
from fastapi import APIRouter, HTTPException, Depends
from pydantic import BaseModel
import aio_pika

from app.workers.exporter_worker import ExporterWorker
from app.config.rabbitmq import get_rabbitmq_connection
from app.models.messages import ImportRequestMessage
from app.dependencies.rate_limiter import check_demo_rate_limit

logger = logging.getLogger(__name__)

router = APIRouter()

class DemoRequest(BaseModel):
    urls: List[str]

@router.post("/demo", dependencies=[Depends(check_demo_rate_limit)])
async def request_demo(payload: DemoRequest):
    if len(payload.urls) > 3:
        raise HTTPException(status_code=400, detail="Máximo de 3 URLs permitidas para a demo.")
        
    try:
        connection = await get_rabbitmq_connection()
        async with connection:
            channel = await connection.channel()
            
            for url in payload.urls:
                msg_data = ImportRequestMessage(
                    target_url=url,
                    tenant_id="demo_tenant",
                    product_id="demo"
                )
                
                message = aio_pika.Message(
                    body=msg_data.model_dump_json(by_alias=True).encode(),
                    priority=10,
                    delivery_mode=aio_pika.DeliveryMode.PERSISTENT
                )
                
                await channel.default_exchange.publish(
                    message,
                    routing_key="ecommerce_demo"
                )
                
        return {"status": "enviado_para_fila"}
    except Exception as e:
        logger.error(f"Erro ao publicar demo na fila: {e}")
        raise HTTPException(status_code=500, detail="Erro interno ao processar a solicitação")

@router.get("/export")
async def export_data(tenant_id: str, platform: str = "shopify"):
    try:
        logger.info(f"Solicitação de exportação recebida para tenant: {tenant_id}, plataforma: {platform}")
        exporter = ExporterWorker(tenant_id=tenant_id, platform=platform)
        await exporter.export()
        
        return {"status": "success", "message": f"Exportação concluída para a plataforma {platform}. Arquivo gerado no servidor."}
    except Exception as e:
        logger.error(f"Erro ao exportar dados: {e}")
        raise HTTPException(status_code=500, detail="Erro interno durante a exportação")

@router.get("/health")
async def health_check():
    return {"status": "ok"}