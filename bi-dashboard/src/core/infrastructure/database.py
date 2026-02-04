"""
Database Infrastructure Module (ASYNC)
Configuração de conexão SQLAlchemy Assíncrona para SQL Server
"""

import os
from typing import Optional
from sqlalchemy.ext.asyncio import create_async_engine, AsyncEngine, AsyncSession, async_sessionmaker
from sqlalchemy import text
from sqlalchemy.pool import QueuePool
from dotenv import load_dotenv
import urllib.parse
import asyncio

load_dotenv()

class DatabaseConfig:
    """Configuração centralizada do banco de dados SQL Server"""
    
    def __init__(self):
        self.server = os.getenv("DB_SERVER", "localhost")
        self.database = os.getenv("DB_NAME", "GregCompanyDB")
        self.username = os.getenv("DB_USERNAME", "sa")
        self.password = os.getenv("DB_PASSWORD", "")
        # Nota: O driver ODBC do sistema continua o mesmo, mas o wrapper muda
        self.driver = os.getenv("DB_DRIVER", "ODBC Driver 17 for SQL Server")
        self.port = os.getenv("DB_PORT", "1433")
        
    def get_connection_string(self) -> str:
        """
        Gera a connection string para SQLAlchemy ASYNC (aioodbc)
        Muda de mssql+pyodbc para mssql+aioodbc
        """
        encoded_password = urllib.parse.quote_plus(self.password)
        encoded_username = urllib.parse.quote_plus(self.username)
        encoded_driver = urllib.parse.quote_plus(self.driver)
        
        return (
            f"mssql+aioodbc://{encoded_username}:{encoded_password}"
            f"@{self.server}:{self.port}/{self.database}"
            f"?driver={encoded_driver}"
            f"&TrustServerCertificate=yes"
        )

class DatabaseConnection:
    """
    Gerenciador de conexão Async com SQL Server
    """
    
    _instance: Optional['DatabaseConnection'] = None
    _engine: Optional[AsyncEngine] = None
    _session_factory: Optional[async_sessionmaker] = None
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance
    
    def __init__(self):
        if self._engine is None:
            self._initialize_engine()
    
    def _initialize_engine(self):
        config = DatabaseConfig()
        connection_string = config.get_connection_string()
        
        # Cria engine assíncrono
        self._engine = create_async_engine(
            connection_string,
            poolclass=QueuePool,
            pool_size=5,
            max_overflow=10,
            pool_pre_ping=True,
            echo=False,
        )
        
        # Cria fábrica de sessões assíncronas
        self._session_factory = async_sessionmaker(
            bind=self._engine,
            class_=AsyncSession,
            autocommit=False,
            autoflush=False,
            expire_on_commit=False # Importante para async
        )
        
        print(f"✅ Async Database Engine inicializado: {config.database}@{config.server}")
    
    @property
    def engine(self) -> AsyncEngine:
        if self._engine is None:
            self._initialize_engine()
        return self._engine
    
    def get_session(self) -> AsyncSession:
        """
        Cria uma nova sessão Async
        Uso: async with db.get_session() as session:
        """
        if self._session_factory is None:
            self._initialize_engine()
        return self._session_factory()
    
    async def test_connection(self) -> bool:
        """Testa conexão de forma assíncrona"""
        try:
            # Em async, precisamos pegar uma conexão do engine
            async with self.engine.connect() as conn:
                result = await conn.execute(text("SELECT 1"))
                print("✅ Conexão Async com SQL Server OK!")
                return True
        except Exception as e:
            print(f"❌ Erro ao conectar no SQL Server: {e}")
            return False
            
    async def close(self):
        if self._engine:
            await self._engine.dispose()
            print("🔒 Engine Async fechado")

# Singleton
db_connection = DatabaseConnection()

# Helper para injeção de dependência no FastAPI
async def get_db_session() -> AsyncSession:
    async with db_connection.get_session() as session:
        yield session