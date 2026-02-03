"""
WebSocket Infrastructure
Gerenciamento de conexões WebSocket para comunicação real-time com frontend
Baseado no padrão SignalR usado no sistema principal
"""

from fastapi import WebSocket, WebSocketDisconnect
from typing import Dict, Set, Any, Callable, Optional
from datetime import datetime
import json
import asyncio
import logging

logger = logging.getLogger(__name__)


class WebSocketConnection:
    """Representa uma conexão WebSocket individual"""
    
    def __init__(self, websocket: WebSocket, client_id: str):
        self.websocket = websocket
        self.client_id = client_id
        self.connected_at = datetime.now()
        self.subscribed_methods: Set[str] = set()
    
    async def send(self, message: dict):
        """Envia mensagem para este cliente"""
        try:
            await self.websocket.send_json(message)
        except Exception as e:
            logger.error(f"Erro ao enviar mensagem para {self.client_id}: {e}")
            raise


class WebSocketHub:
    """
    Hub de WebSocket (similar ao SignalR Hub)
    Gerencia conexões e broadcasting para um domínio específico
    """
    
    def __init__(self, hub_name: str):
        self.hub_name = hub_name
        self.active_connections: Dict[str, WebSocketConnection] = {}
        self.event_handlers: Dict[str, Callable] = {}
    
    async def connect(self, websocket: WebSocket, client_id: str) -> WebSocketConnection:
        """Conecta cliente ao hub"""
        await websocket.accept()
        
        connection = WebSocketConnection(websocket, client_id)
        self.active_connections[client_id] = connection
        
        logger.info(f"🔌 Cliente {client_id} conectado ao hub '{self.hub_name}'")
        logger.info(f"📊 Total de conexões no hub '{self.hub_name}': {len(self.active_connections)}")
        
        # Notifica conexão
        await self.broadcast({
            "type": "ClientConnected",
            "hubName": self.hub_name,
            "clientId": client_id,
            "timestamp": datetime.now().isoformat()
        }, exclude_client=client_id)
        
        return connection
    
    def disconnect(self, client_id: str):
        """Desconecta cliente do hub"""
        if client_id in self.active_connections:
            del self.active_connections[client_id]
            logger.info(f"🔌 Cliente {client_id} desconectado do hub '{self.hub_name}'")
            logger.info(f"📊 Conexões restantes: {len(self.active_connections)}")
    
    async def send_to_client(self, client_id: str, method_name: str, data: Any):
        """Envia mensagem para cliente específico"""
        if client_id not in self.active_connections:
            logger.warning(f"Cliente {client_id} não encontrado no hub '{self.hub_name}'")
            return
        
        message = {
            "type": "Invoke",
            "hubName": self.hub_name,
            "method": method_name,
            "data": data,
            "timestamp": datetime.now().isoformat()
        }
        
        try:
            await self.active_connections[client_id].send(message)
        except Exception as e:
            logger.error(f"Erro ao enviar para {client_id}: {e}")
            self.disconnect(client_id)
    
    async def broadcast(self, data: dict, exclude_client: Optional[str] = None):
        """
        Broadcast otimizado para todos os clientes do hub usando concorrência.
        """
        if not self.active_connections:
            return

        # Criamos uma lista de tarefas de envio para processar em paralelo
        tasks = []
        client_ids = []

        for client_id, connection in self.active_connections.items():
            if exclude_client and client_id == exclude_client:
                continue
            
            # Adicionamos a corrotina na lista (sem dar o await ainda)
            tasks.append(connection.send(data))
            client_ids.append(client_id)
        
        if not tasks:
            return

        results = await asyncio.gather(*tasks, return_exceptions=True)

        # Analisamos os resultados para desconectar quem falhou
        disconnected_clients = []
        for i, result in enumerate(results):
            if isinstance(result, Exception):
                client_id = client_ids[i]
                logger.error(f"Erro no broadcast paralelo para {client_id}: {result}")
                disconnected_clients.append(client_id)
        
        # Limpeza de conexões mortas
        for client_id in disconnected_clients:
            self.disconnect(client_id)
    
    async def broadcast_method(self, method_name: str, data: Any, exclude_client: Optional[str] = None):
        """Broadcast de método específico (padrão SignalR)"""
        message = {
            "type": "Invoke",
            "hubName": self.hub_name,
            "method": method_name,
            "data": data,
            "timestamp": datetime.now().isoformat()
        }
        
        await self.broadcast(message, exclude_client)
    
    def register_handler(self, method_name: str, handler: Callable):
        """Registra handler para método invocado pelo cliente"""
        self.event_handlers[method_name] = handler
        logger.info(f"📝 Handler registrado para '{method_name}' no hub '{self.hub_name}'")
    
    async def handle_client_invoke(self, client_id: str, method_name: str, *args):
        """Processa invocação de método pelo cliente"""
        if method_name not in self.event_handlers:
            logger.warning(f"Método '{method_name}' não registrado no hub '{self.hub_name}'")
            return
        
        try:
            handler = self.event_handlers[method_name]
            await handler(client_id, *args)
        except Exception as e:
            logger.error(f"Erro ao executar handler '{method_name}': {e}")
    
    def get_connection_count(self) -> int:
        """Retorna número de conexões ativas"""
        return len(self.active_connections)


class WebSocketManager:
    """
    Gerenciador global de WebSocket Hubs
    Similar ao pattern usado no SignalR com múltiplos Hubs
    """
    
    def __init__(self):
        self.hubs: Dict[str, WebSocketHub] = {}
        self._lock = asyncio.Lock()
    
    def create_hub(self, hub_name: str) -> WebSocketHub:
        """Cria um novo hub"""
        if hub_name not in self.hubs:
            self.hubs[hub_name] = WebSocketHub(hub_name)
            logger.info(f"🔧 Hub '{hub_name}' criado")
        return self.hubs[hub_name]
    
    def get_hub(self, hub_name: str) -> Optional[WebSocketHub]:
        """Retorna hub existente"""
        return self.hubs.get(hub_name)
    
    async def connect_to_hub(self, hub_name: str, websocket: WebSocket, client_id: str) -> WebSocketConnection:
        """Conecta cliente a um hub específico"""
        async with self._lock:
            hub = self.get_hub(hub_name)
            if not hub:
                hub = self.create_hub(hub_name)
            
            return await hub.connect(websocket, client_id)
    
    def disconnect_from_hub(self, hub_name: str, client_id: str):
        """Desconecta cliente de um hub"""
        hub = self.get_hub(hub_name)
        if hub:
            hub.disconnect(client_id)
    
    def get_total_connections(self) -> int:
        """Retorna total de conexões em todos os hubs"""
        return sum(hub.get_connection_count() for hub in self.hubs.values())
    
    def get_hubs_status(self) -> dict:
        """Retorna status de todos os hubs"""
        return {
            hub_name: {
                "connections": hub.get_connection_count(),
                "handlers": list(hub.event_handlers.keys())
            }
            for hub_name, hub in self.hubs.items()
        }


# Singleton global
ws_manager = WebSocketManager()


# Helper functions para compatibilidade
def get_hub(hub_name: str) -> Optional[WebSocketHub]:
    """Helper para obter hub"""
    return ws_manager.get_hub(hub_name)


def create_hub(hub_name: str) -> WebSocketHub:
    """Helper para criar hub"""
    return ws_manager.create_hub(hub_name)
