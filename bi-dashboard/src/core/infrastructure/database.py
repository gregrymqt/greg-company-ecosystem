"""
Database Infrastructure Module
Configuração de conexão SQLAlchemy para SQL Server
Baseado nas entidades do ApiDbContext.cs
"""

import os
from typing import Optional
from sqlalchemy import create_engine, Engine, text
from sqlalchemy.orm import sessionmaker, Session
from sqlalchemy.pool import QueuePool
from dotenv import load_dotenv
import urllib.parse

load_dotenv()


class DatabaseConfig:
    """Configuração centralizada do banco de dados SQL Server"""
    
    def __init__(self):
        self.server = os.getenv("SQL_SERVER", "localhost")
        self.database = os.getenv("SQL_DATABASE", "GregCompanyDB")
        self.username = os.getenv("SQL_USERNAME", "sa")
        self.password = os.getenv("SQL_PASSWORD", "")
        self.driver = os.getenv("SQL_DRIVER", "ODBC Driver 17 for SQL Server")
        self.port = os.getenv("SQL_PORT", "1433")
        
    def get_connection_string(self) -> str:
        """
        Gera a connection string para SQLAlchemy com SQL Server
        Formato: mssql+pyodbc://user:pass@server:port/database?driver=...
        """
        # Encode password para URL safety
        encoded_password = urllib.parse.quote_plus(self.password)
        encoded_username = urllib.parse.quote_plus(self.username)
        encoded_driver = urllib.parse.quote_plus(self.driver)
        
        return (
            f"mssql+pyodbc://{encoded_username}:{encoded_password}"
            f"@{self.server}:{self.port}/{self.database}"
            f"?driver={encoded_driver}"
            f"&TrustServerCertificate=yes"
        )


class DatabaseConnection:
    """
    Gerenciador de conexão com SQL Server usando SQLAlchemy
    Implementa Singleton pattern para reutilizar engine
    """
    
    _instance: Optional['DatabaseConnection'] = None
    _engine: Optional[Engine] = None
    _session_factory: Optional[sessionmaker] = None
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance
    
    def __init__(self):
        if self._engine is None:
            self._initialize_engine()
    
    def _initialize_engine(self):
        """Cria e configura o engine SQLAlchemy"""
        config = DatabaseConfig()
        connection_string = config.get_connection_string()
        
        self._engine = create_engine(
            connection_string,
            poolclass=QueuePool,
            pool_size=5,
            max_overflow=10,
            pool_pre_ping=True,  # Verifica conexão antes de usar
            pool_recycle=3600,   # Recicla conexões a cada hora
            echo=False,          # Mude para True para debug
        )
        
        self._session_factory = sessionmaker(
            bind=self._engine,
            autocommit=False,
            autoflush=False
        )
        
        print(f"✅ Database Engine inicializado: {config.database}@{config.server}")
    
    @property
    def engine(self) -> Engine:
        """Retorna o engine SQLAlchemy"""
        if self._engine is None:
            self._initialize_engine()
        return self._engine
    
    def get_session(self) -> Session:
        """
        Cria uma nova sessão SQLAlchemy
        Use com context manager: with db.get_session() as session:
        """
        if self._session_factory is None:
            self._initialize_engine()
        return self._session_factory()
    
    def test_connection(self) -> bool:
        """Testa a conexão com o banco de dados"""
        try:
            with self.engine.connect() as conn:
                result = conn.execute(text("SELECT 1"))
                result.fetchone()
                print("✅ Conexão com SQL Server OK!")
                return True
        except Exception as e:
            print(f"❌ Erro ao conectar no SQL Server: {e}")
            return False
    
    def close(self):
        """Fecha todas as conexões do pool"""
        if self._engine is not None:
            self._engine.dispose()
            print("🔒 Pool de conexões fechado")


# Singleton instance global
db_connection = DatabaseConnection()


def get_db_session() -> Session:
    """
    Helper function para obter sessão do banco
    Uso recomendado:
    
    with get_db_session() as session:
        results = session.execute(text("SELECT * FROM Payments"))
    """
    return db_connection.get_session()


def get_db_engine() -> Engine:
    """
    Helper function para obter engine do banco
    Útil para pandas.read_sql ou queries diretas
    """
    return db_connection.engine


if __name__ == "__main__":
    # Teste de conexão
    print("🔧 Testando conexão com SQL Server...")
    db_connection.test_connection()
