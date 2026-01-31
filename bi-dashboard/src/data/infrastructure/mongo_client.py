"""
MongoDB Client Infrastructure
Configuração de conexão PyMongo para o banco de tickets (SupportTicketDocument)
"""

import os
from typing import Optional
from pymongo import MongoClient
from pymongo.database import Database
from pymongo.collection import Collection
from dotenv import load_dotenv

load_dotenv()


class MongoConfig:
    """Configuração centralizada do MongoDB"""
    
    def __init__(self):
        self.host = os.getenv("MONGO_HOST", "localhost")
        self.port = int(os.getenv("MONGO_PORT", "27017"))
        self.database_name = os.getenv("MONGO_DATABASE", "GregCompanyMongo")
        self.username = os.getenv("MONGO_USERNAME", "")
        self.password = os.getenv("MONGO_PASSWORD", "")
        
    def get_connection_string(self) -> str:
        """
        Gera a connection string para PyMongo
        Se tiver username/password: mongodb://user:pass@host:port/
        Senão: mongodb://host:port/
        """
        if self.username and self.password:
            return f"mongodb://{self.username}:{self.password}@{self.host}:{self.port}/"
        else:
            return f"mongodb://{self.host}:{self.port}/"


class MongoConnection:
    """
    Gerenciador de conexão com MongoDB usando PyMongo
    Implementa Singleton pattern
    """
    
    _instance: Optional['MongoConnection'] = None
    _client: Optional[MongoClient] = None
    _database: Optional[Database] = None
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance
    
    def __init__(self):
        if self._client is None:
            self._initialize_client()
    
    def _initialize_client(self):
        """Cria e configura o MongoClient"""
        config = MongoConfig()
        connection_string = config.get_connection_string()
        
        self._client = MongoClient(
            connection_string,
            serverSelectionTimeoutMS=5000,  # Timeout de 5 segundos
            maxPoolSize=10,
            minPoolSize=1,
        )
        
        self._database = self._client[config.database_name]
        
        print(f"✅ MongoDB Client inicializado: {config.database_name}@{config.host}")
    
    @property
    def client(self) -> MongoClient:
        """Retorna o MongoClient"""
        if self._client is None:
            self._initialize_client()
        return self._client
    
    @property
    def database(self) -> Database:
        """Retorna o database do MongoDB"""
        if self._database is None:
            self._initialize_client()
        return self._database
    
    def get_collection(self, collection_name: str) -> Collection:
        """
        Retorna uma collection específica
        Ex: mongo.get_collection("support_tickets")
        """
        return self.database[collection_name]
    
    def test_connection(self) -> bool:
        """Testa a conexão com o MongoDB"""
        try:
            # Ping no servidor
            self.client.admin.command('ping')
            print("✅ Conexão com MongoDB OK!")
            return True
        except Exception as e:
            print(f"❌ Erro ao conectar no MongoDB: {e}")
            return False
    
    def list_collections(self) -> list:
        """Lista todas as collections do database"""
        try:
            collections = self.database.list_collection_names()
            print(f"📁 Collections disponíveis: {collections}")
            return collections
        except Exception as e:
            print(f"❌ Erro ao listar collections: {e}")
            return []
    
    def close(self):
        """Fecha a conexão com MongoDB"""
        if self._client is not None:
            self._client.close()
            print("🔒 Conexão MongoDB fechada")


# Singleton instance global
mongo_connection = MongoConnection()


def get_mongo_db() -> Database:
    """
    Helper function para obter database do MongoDB
    Uso:
    
    db = get_mongo_db()
    tickets = db['support_tickets'].find()
    """
    return mongo_connection.database


def get_mongo_collection(collection_name: str) -> Collection:
    """
    Helper function para obter collection do MongoDB
    Uso:
    
    tickets_collection = get_mongo_collection("support_tickets")
    results = tickets_collection.find({"status": "Open"})
    """
    return mongo_connection.get_collection(collection_name)


if __name__ == "__main__":
    # Teste de conexão
    print("🔧 Testando conexão com MongoDB...")
    mongo_connection.test_connection()
    mongo_connection.list_collections()
