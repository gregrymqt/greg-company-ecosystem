"""
MongoDB Infrastructure Module (ASYNC)
Usando Motor (Motor Asyncio)
"""

import os
from typing import Optional
from motor.motor_asyncio import AsyncIOMotorClient, AsyncIOMotorDatabase
from dotenv import load_dotenv
import asyncio

load_dotenv()

class MongoDBConfig:
    def __init__(self):
        self.connection_string = os.getenv("MONGO_CONNECTION_STRING", "mongodb://localhost:27017/")
        self.database_name = os.getenv("MONGO_DATABASE", "GregCompanyMongo")

class MongoDBConnection:
    _instance: Optional['MongoDBConnection'] = None
    _client: Optional[AsyncIOMotorClient] = None
    _database: Optional[AsyncIOMotorDatabase] = None
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance
    
    def __init__(self):
        if self._client is None:
            self._initialize_client()
    
    def _initialize_client(self):
        config = MongoDBConfig()
        # Motor Client é nativamente async, mas a inicialização é síncrona (lazy)
        self._client = AsyncIOMotorClient(config.connection_string)
        self._database = self._client[config.database_name]
        print(f"✅ Motor (Mongo Async) inicializado: {config.database_name}")
    
    @property
    def database(self) -> AsyncIOMotorDatabase:
        if self._database is None:
            self._initialize_client()
        return self._database
    
    async def test_connection(self) -> bool:
        try:
            # 'ping' precisa de await no motor
            await self._client.admin.command('ping')
            print("✅ Conexão Async com MongoDB OK!")
            return True
        except Exception as e:
            print(f"❌ Erro ao conectar no MongoDB: {e}")
            return False
            
    def close(self):
        if self._client:
            self._client.close()
            print("🔒 Conexão MongoDB fechada")

mongo_connection = MongoDBConnection()

def get_mongo_db() -> AsyncIOMotorDatabase:
    return mongo_connection.database