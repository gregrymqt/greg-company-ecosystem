"""
MongoDB Infrastructure Module
Configuração de conexão MongoDB
"""

import os
from typing import Optional
from pymongo import MongoClient
from pymongo.database import Database
from dotenv import load_dotenv

load_dotenv()


class MongoDBConfig:
    """Configuração centralizada do MongoDB"""
    
    def __init__(self):
        self.connection_string = os.getenv(
            "MONGO_CONNECTION_STRING", 
            "mongodb://localhost:27017/"
        )
        self.database_name = os.getenv("MONGO_DATABASE", "GregCompanyMongo")


class MongoDBConnection:
    """
    Gerenciador de conexão com MongoDB
    Implementa Singleton pattern
    """
    
    _instance: Optional['MongoDBConnection'] = None
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
        """Cria e configura o client MongoDB"""
        config = MongoDBConfig()
        
        self._client = MongoClient(config.connection_string)
        self._database = self._client[config.database_name]
        
        print(f"✅ MongoDB Client inicializado: {config.database_name}")
    
    @property
    def database(self) -> Database:
        """Retorna a database MongoDB"""
        if self._database is None:
            self._initialize_client()
        return self._database
    
    def test_connection(self) -> bool:
        """Testa a conexão com o MongoDB"""
        try:
            self._client.admin.command('ping')
            print("✅ Conexão com MongoDB OK!")
            return True
        except Exception as e:
            print(f"❌ Erro ao conectar no MongoDB: {e}")
            return False
    
    def close(self):
        """Fecha a conexão com MongoDB"""
        if self._client is not None:
            self._client.close()
            print("🔒 Conexão MongoDB fechada")


# Singleton instance global
mongo_connection = MongoDBConnection()


def get_mongo_db() -> Database:
    """
    Helper function para obter database do MongoDB
    """
    return mongo_connection.database


if __name__ == "__main__":
    # Teste de conexão
    print("🔧 Testando conexão com MongoDB...")
    mongo_connection.test_connection()
