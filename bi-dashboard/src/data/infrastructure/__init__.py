"""
Infrastructure Package
Exporta os módulos de infraestrutura de dados
"""

from .database import (
    DatabaseConfig,
    DatabaseConnection,
    db_connection,
    get_db_session,
    get_db_engine
)

from .mongo_client import (
    MongoConfig,
    MongoConnection,
    mongo_connection,
    get_mongo_db,
    get_mongo_collection
)

__all__ = [
    # Database SQL Server
    'DatabaseConfig',
    'DatabaseConnection',
    'db_connection',
    'get_db_session',
    'get_db_engine',
    # MongoDB
    'MongoConfig',
    'MongoConnection',
    'mongo_connection',
    'get_mongo_db',
    'get_mongo_collection',
]
