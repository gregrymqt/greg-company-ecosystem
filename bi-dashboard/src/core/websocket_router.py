import logging
import uuid
import json
from typing import Optional, Any, Dict
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Query

# Importa sua infraestrutura (assumindo que está em src/core/infrastructure/websocket.py)
from .infrastructure.websocket import ws_manager 
from .enums.hub_enums import AppHubs

logger = logging.getLogger(__name__)

async def handle_client_message(hub_name: str, client_id: str, data: Dict[str, Any]) -> None:
    """
    Roteador Central: Recebe o JSON do cliente e direciona para o Handler correto.
    """
    message_type = data.get("type") or data.get("method") # Suporta ambos os padrões
    
    # Caso 1: Cliente quer executar uma ação (Ex: ForceUpdate)
    if message_type == "Invoke":
        method_name = data.get("method")
        args = data.get("args", [])
        
        hub = ws_manager.get_hub(hub_name)
        if hub:
            # O Hub vai procurar se existe um Handler registrado para esse método
            # Ex: Se for Storage, chama StorageHubHandlers.handle_client_invoke
            await hub.handle_client_invoke(client_id, method_name, *args)

    # Caso 2: Cliente quer assinar um tópico (Ex: SubscribeToRevenue)
    elif message_type == "Subscribe":
        method_name = data.get("method")
        hub = ws_manager.get_hub(hub_name)
        if hub and client_id in hub.active_connections:
            hub.active_connections[client_id].subscribed_methods.add(method_name)
            logger.info(f"✅ {client_id} inscrito em {method_name} no hub {hub_name}")

    # Caso 3: Ping/Pong (Heartbeat)
    elif message_type == "Ping":
        hub = ws_manager.get_hub(hub_name)
        if hub:
            await hub.send_to_client(client_id, "Pong", {"timestamp": data.get("timestamp")})

def setup_websocket_routes(app: FastAPI) -> None:
    """
    Configura a rota única /ws no FastAPI.
    O cliente decide onde conectar via URL: ws://localhost:8000/ws?hub=Storage
    """
    
    @app.websocket("/ws")
    async def websocket_endpoint(
        websocket: WebSocket,
        client_id: Optional[str] = Query(None),
        hub: Optional[str] = Query(None) # O cliente DEVE mandar o nome do hub
    ):
        # 1. Validação de ID
        if not client_id:
            client_id = str(uuid.uuid4())
            
        # 2. Definição do Hub Alvo
        target_hub = hub if hub else AppHubs.NOTIFICATIONS.value
        
        # 3. Conexão via Infraestrutura (ws_manager)
        # O manager já faz o websocket.accept() internamente
        try:
            connection = await ws_manager.connect_to_hub(target_hub, websocket, client_id)
            
            # 4. Mensagem de Boas-vindas
            await connection.send({
                "type": "Connected",
                "hub": target_hub,
                "clientId": client_id,
                "message": f"Conectado com sucesso ao hub {target_hub}"
            })
            
            # 5. O LOOP ETERNO (Obrigatório no Backend!)
            # Mantém a conexão aberta esperando mensagens do React
            while True:
                try:
                    # Espera o próximo JSON chegar
                    data = await websocket.receive_json()
                    
                    # Processa a mensagem
                    await handle_client_message(target_hub, client_id, data)
                    
                except json.JSONDecodeError:
                    logger.warning(f"Recebido dados inválidos de {client_id}")
                    continue
                
        except WebSocketDisconnect:
            logger.info(f"🔌 Cliente {client_id} desconectou de {target_hub}")
            ws_manager.disconnect_from_hub(target_hub, client_id)
            
        except Exception as e:
            logger.error(f"❌ Erro crítico no Socket ({target_hub}): {e}")
            ws_manager.disconnect_from_hub(target_hub, client_id)
            
    # Endpoint auxiliar para Debug
    @app.get("/ws/status")
    async def websocket_status():
        return {
            "active_connections": ws_manager.get_total_connections(),
            "hubs_details": ws_manager.get_hubs_status()
        }