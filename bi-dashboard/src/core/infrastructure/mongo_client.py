import os
from typing import Optional
from motor.motor_asyncio import AsyncIOMotorClient, AsyncIOMotorDatabase
from dotenv import load_dotenv
import asyncio
# Importe o pymongo para definir a ordem (ASCENDING/DESCENDING)
import pymongo 

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
            await self._client.admin.command('ping')
            print("✅ Conexão Async com MongoDB OK!")
            return True
        except Exception as e:
            print(f"❌ Erro ao conectar no MongoDB: {e}")
            return False

    # === NOVO MÉTODO: Criação Automática de Índices ===
    async def create_indexes(self):
        """
        Cria índices otimizados para as features.
        Isso deve ser chamado no startup da aplicação.
        """
        try:
            db = self.database
            
            # --- Feature: Support ---
            # Índice composto para filtrar por Status e ordenar por Data (Alta Performance)
            await db["support_tickets"].create_index(
                [("Status", pymongo.ASCENDING), ("CreatedAt", pymongo.DESCENDING)],
                name="idx_status_created_at",
                background=True # Não trava o banco enquanto cria
            )
            
            print("⚡ Índices do MongoDB verificados/criados com sucesso.")
        except Exception as e:
            print(f"⚠️ Erro ao criar índices no Mongo: {e}")

    def close(self):
        if self._client:
            self._client.close()
            print("🔒 Conexão MongoDB fechada")

mongo_connection = MongoDBConnection()

def get_mongo_db() -> AsyncIOMotorDatabase:
    return mongo_connection.database