import logging
from typing import Any
from ...core.infrastructure.websocket import ws_manager
from ...core.infrastructure.redis_client import delete_key
from ...core.enums.hub_enums import AppHubs
from .service import create_storage_service

logger = logging.getLogger(__name__)

class StorageHubHandlers:
    def __init__(self):
        self.feature_name = AppHubs.STORAGE.value

    async def handle_client_invoke(self, client_id: str, method: str, *args: Any):
        if method == "ForceUpdate":
            await self.handle_force_update(client_id)
        elif method == "GetInitialData":
            await self.handle_initial_data(client_id)

    async def handle_initial_data(self, client_id: str):
        try:
            service = create_storage_service()
            stats = await service.get_storage_overview(use_cache=True)
            
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.send_to_client(client_id, "StorageStatsUpdate", {
                    "stats": stats.model_dump(),
                    "type": "Initial"
                })
        except Exception as e:
            logger.error(f"❌ Erro Storage InitialData: {e}")

    async def handle_force_update(self, client_id: str):
        logger.info(f"🔄 ForceUpdate Storage solicitado por {client_id}")
        try:
            await self.broadcast_storage_update(type="Manual")
            
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.send_to_client(client_id, "UpdateProcessed", {"status": "ok"})
        except Exception as e:
            logger.error(f"❌ Erro ForceUpdate Storage: {e}")

    # --- Método Público para o Loop de Background ---

    async def broadcast_storage_update(self, type: str = "Auto"):
        try:
            service = create_storage_service()
            
            # Limpa cache explicitamente para garantir frescor no Auto Update
            await delete_key("storage:overview:stats")
            
            stats = await service.get_storage_overview(use_cache=False)
            
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.broadcast("StorageStatsUpdate", {
                    "stats": stats.model_dump(),
                    "type": type
                })
        except Exception as e:
            logger.error(f"Erro broadcast Storage: {e}")

async def setup_storage_handlers():
    return StorageHubHandlers()