"""
Core Infrastructure Module
Exporta configurações de banco de dados e WebSocket
"""

from .database import get_db_session, get_db_engine, DatabaseConnection
from .mongo_client import get_mongo_db, MongoDBConnection
from .websocket import (
    ws_manager,
    WebSocketManager,
    WebSocketHub,
    WebSocketConnection,
    get_hub,
    create_hub,
)

__all__ = [
    # Database
    'get_db_session',
    'get_db_engine',
    'DatabaseConnection',
    
    # MongoDB
    'get_mongo_db',
    'MongoDBConnection',
    
    # WebSocket
    'ws_manager',
    'WebSocketManager',
    'WebSocketHub',
    'WebSocketConnection',
    'get_hub',
    'create_hub',
]
