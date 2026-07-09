import logging
from datetime import datetime, timezone, timedelta
import fastapi
from app.config.database import db

logger = logging.getLogger(__name__)

async def check_demo_rate_limit(request: fastapi.Request):
    if not request.client or not request.client.host:
        client_ip = "unknown"
    else:
        client_ip = request.client.host
        
    collection = db.client["ecommerce"]["demo_rate_limits"]
    
    one_hour_ago = datetime.now(timezone.utc) - timedelta(hours=1)
    
    count = await collection.count_documents({
        "ip": client_ip,
        "created_at": {"$gte": one_hour_ago}
    })
    
    if count >= 5:
        logger.warning(f"Rate limit bloqueou o IP: {client_ip}. Requisições na última hora: {count}")
        raise fastapi.HTTPException(
            status_code=429, 
            detail="Limite de demonstração excedido para o seu IP. Tente novamente mais tarde."
        )
        
    await collection.insert_one({
        "ip": client_ip,
        "created_at": datetime.now(timezone.utc)
    })
