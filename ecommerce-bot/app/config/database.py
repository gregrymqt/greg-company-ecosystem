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
    
    logging.info("Conectado ao MongoDB e índices verificados!")

async def close_mongo_connection():
    if db.client:
        db.client.close()
        logging.info("Conexão com MongoDB fechada.")