import os
import logging
from motor.motor_asyncio import AsyncIOMotorClient

class Database:
    client: AsyncIOMotorClient = None

db = Database()

from app.config.settings import settings

async def connect_to_mongo():
    mongo_uri = settings.MONGO_URI
    db.client = AsyncIOMotorClient(mongo_uri)
    
    # Criar índices
    collection = db.client["ecommerce"]["products"]
    await collection.create_index("status")
    await collection.create_index([("tenant_id", 1), ("sku", 1)], unique=True)
    
    # Criar índice TTL de 1 hora (3600 segundos) para Rate Limiting
    await db.client["ecommerce"]["demo_rate_limits"].create_index("created_at", expireAfterSeconds=3600)
    
    logging.info("Conectado ao MongoDB e índices verificados!")

async def close_mongo_connection():
    if db.client:
        db.client.close()
        logging.info("Conexão com MongoDB fechada.")