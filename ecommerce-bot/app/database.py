import os
import logging
from motor.motor_asyncio import AsyncIOMotorClient

class Database:
    client: AsyncIOMotorClient = None

db = Database()

async def connect_to_mongo():
    mongo_uri = os.getenv('MONGO_URI', "mongodb://localhost:27017")
    db.client = AsyncIOMotorClient(mongo_uri)
    
    # Criar índices
    collection = db.client["ecommerce"]["products"]
    await collection.create_index("status")
    await collection.create_index("sku", unique=True)
    
    logging.info("Conectado ao MongoDB e índices verificados!")

async def close_mongo_connection():
    if db.client:
        db.client.close()
        logging.info("Conexão com MongoDB fechada.")