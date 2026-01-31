"""
WebSocket Server
Configuração de rotas e endpoints WebSocket
"""

from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Depends, Query
from typing import Optional
import logging
import uuid
import asyncio
import json

from .infrastructure.websocket import ws_manager, WebSocketHub
from .enums.hub_enums import AppHubs

logger = logging.getLogger(__name__)


def setup_websocket_routes(app: FastAPI):
    """
    Configura todas as rotas WebSocket no app FastAPI
    
    Args:
        app: Instância do FastAPI
    """
    
    @app.websocket("/ws")
    async def websocket_general_endpoint(
        websocket: WebSocket,
        client_id: Optional[str] = Query(None)
    ):
        """
        WebSocket geral para notificações
        Similar ao hub de notificações do sistema principal
        """
        if not client_id:
            client_id = str(uuid.uuid4())
        
        hub_name = "notifications"
        connection = await ws_manager.connect_to_hub(hub_name, websocket, client_id)
        
        try:
            # Envia mensagem de boas-vindas
            await connection.send({
                "type": "Connected",
                "hubName": hub_name,
                "clientId": client_id,
                "message": "Conectado ao BI Dashboard WebSocket",
                "timestamp": connection.connected_at.isoformat()
            })
            
            # Loop de recebimento
            while True:
                data = await websocket.receive_json()
                
                # Processa comando
                await handle_client_message(hub_name, client_id, data)
                
        except WebSocketDisconnect:
            logger.info(f"Cliente {client_id} desconectou de {hub_name}")
        except Exception as e:
            logger.error(f"Erro no WebSocket geral: {e}")
        finally:
            ws_manager.disconnect_from_hub(hub_name, client_id)
    
    
    @app.websocket("/hubs/claims")
    async def websocket_claims_hub(
        websocket: WebSocket,
        client_id: Optional[str] = Query(None)
    ):
        """
        Hub WebSocket para Claims Analytics
        Envia atualizações de KPIs, novas claims, etc
        """
        if not client_id:
            client_id = str(uuid.uuid4())
        
        hub_name = "claims"
        connection = await ws_manager.connect_to_hub(hub_name, websocket, client_id)
        
        try:
            # Boas-vindas
            await connection.send({
                "type": "Connected",
                "hubName": hub_name,
                "clientId": client_id,
                "message": "Conectado ao Claims Hub",
                "availableMethods": [
                    "SubscribeToKPIUpdates",
                    "SubscribeToNewClaims",
                    "SubscribeToCriticalClaims"
                ]
            })
            
            # Loop
            while True:
                data = await websocket.receive_json()
                await handle_client_message(hub_name, client_id, data)
                
        except WebSocketDisconnect:
            logger.info(f"Cliente {client_id} desconectou de Claims Hub")
        except Exception as e:
            logger.error(f"Erro no Claims Hub: {e}")
        finally:
            ws_manager.disconnect_from_hub(hub_name, client_id)
    
    
    @app.websocket("/hubs/financial")
    async def websocket_financial_hub(
        websocket: WebSocket,
        client_id: Optional[str] = Query(None)
    ):
        """
        Hub WebSocket para Financial Analytics
        Envia atualizações de pagamentos, receitas, etc
        """
        if not client_id:
            client_id = str(uuid.uuid4())
        
        hub_name = "financial"
        connection = await ws_manager.connect_to_hub(hub_name, websocket, client_id)
        
        try:
            await connection.send({
                "type": "Connected",
                "hubName": hub_name,
                "clientId": client_id,
                "message": "Conectado ao Financial Hub",
                "availableMethods": [
                    "SubscribeToRevenueUpdates",
                    "SubscribeToNewPayments"
                ]
            })
            
            while True:
                data = await websocket.receive_json()
                await handle_client_message(hub_name, client_id, data)
                
        except WebSocketDisconnect:
            logger.info(f"Cliente {client_id} desconectou de Financial Hub")
        except Exception as e:
            logger.error(f"Erro no Financial Hub: {e}")
        finally:
            ws_manager.disconnect_from_hub(hub_name, client_id)
    
    
    @app.websocket("/hubs/subscriptions")
    async def websocket_subscriptions_hub(
        websocket: WebSocket,
        client_id: Optional[str] = Query(None)
    ):
        """Hub WebSocket para Subscriptions Analytics"""
        if not client_id:
            client_id = str(uuid.uuid4())
        
        hub_name = "subscriptions"
        connection = await ws_manager.connect_to_hub(hub_name, websocket, client_id)
        
        try:
            await connection.send({
                "type": "Connected",
                "hubName": hub_name,
                "clientId": client_id,
                "message": "Conectado ao Subscriptions Hub"
            })
            
            while True:
                data = await websocket.receive_json()
                await handle_client_message(hub_name, client_id, data)
                
        except WebSocketDisconnect:
            logger.info(f"Cliente {client_id} desconectou de Subscriptions Hub")
        finally:
            ws_manager.disconnect_from_hub(hub_name, client_id)
    
    
    @app.websocket("/hubs/support")
    async def websocket_support_hub(
        websocket: WebSocket,
        client_id: Optional[str] = Query(None)
    ):
        """Hub WebSocket para Support Analytics"""
        if not client_id:
            client_id = str(uuid.uuid4())
        
        hub_name = "support"
        connection = await ws_manager.connect_to_hub(hub_name, websocket, client_id)
        
        try:
            await connection.send({
                "type": "Connected",
                "hubName": hub_name,
                "clientId": client_id,
                "message": "Conectado ao Support Hub"
            })
            
            while True:
                data = await websocket.receive_json()
                await handle_client_message(hub_name, client_id, data)
                
        except WebSocketDisconnect:
            logger.info(f"Cliente {client_id} desconectou de Support Hub")
        finally:
            ws_manager.disconnect_from_hub(hub_name, client_id)
    
    
    # Endpoint REST para status
    @app.get("/ws/status")
    async def websocket_status():
        """Retorna status de todos os hubs WebSocket"""
        return {
            "totalConnections": ws_manager.get_total_connections(),
            "hubs": ws_manager.get_hubs_status()
        }
    
    
    return app


async def handle_client_message(hub_name: str, client_id: str, data: dict):
    """
    Processa mensagem recebida do cliente
    
    Args:
        hub_name: Nome do hub
        client_id: ID do cliente
        data: Dados recebidos
    """
    message_type = data.get("type", "unknown")
    
    if message_type == "Invoke":
        # Cliente quer invocar um método no servidor
        method_name = data.get("method")
        args = data.get("args", [])
        
        hub = ws_manager.get_hub(hub_name)
        if hub:
            await hub.handle_client_invoke(client_id, method_name, *args)
    
    elif message_type == "Subscribe":
        # Cliente quer se inscrever em um método/evento
        method_name = data.get("method")
        logger.info(f"Cliente {client_id} inscrito em '{method_name}' no hub '{hub_name}'")
        
        # Registra inscrição
        hub = ws_manager.get_hub(hub_name)
        if hub and client_id in hub.active_connections:
            hub.active_connections[client_id].subscribed_methods.add(method_name)
    
    elif message_type == "Unsubscribe":
        # Cliente quer cancelar inscrição
        method_name = data.get("method")
        logger.info(f"Cliente {client_id} desinscrito de '{method_name}' no hub '{hub_name}'")
        
        hub = ws_manager.get_hub(hub_name)
        if hub and client_id in hub.active_connections:
            hub.active_connections[client_id].subscribed_methods.discard(method_name)
    
    else:
        logger.warning(f"Tipo de mensagem desconhecido: {message_type}")
