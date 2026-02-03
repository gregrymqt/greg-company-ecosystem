import logging
from typing import Any
from ...core.infrastructure.websocket import ws_manager
from ...core.infrastructure.redis_client import delete_key
from ...core.enums.hub_enums import AppHubs
from .service import create_content_service

logger = logging.getLogger(__name__)

class ContentHubHandlers:
    def __init__(self):
        self.feature_name = AppHubs.CONTENT.value

    async def handle_client_invoke(self, client_id: str, method: str, *args: Any):
        if method == "ForceUpdate":
            await self.handle_force_update(client_id)
        elif method == "GetInitialData":
            await self.handle_initial_data(client_id)

    async def handle_initial_data(self, client_id: str):
        try:
            service = create_content_service()
            summary = await service.get_content_kpis(use_cache=True)
            
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.send_to_client(client_id, "ContentMetricsUpdate", {
                    "summary": summary.model_dump(),
                    "type": "Initial"
                })
        except Exception as e:
            logger.error(f"❌ Erro Content InitialData: {e}")

    async def handle_force_update(self, client_id: str):
        try:
            await self.broadcast_content_update(type="Manual")
            
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.send_to_client(client_id, "UpdateProcessed", {"status": "ok"})
        except Exception as e:
            logger.error(f"❌ Erro Content ForceUpdate: {e}")

    # --- Método Público para o Loop de Background ---

    async def broadcast_content_update(self, type: str = "Auto"):
        try:
            service = create_content_service()
            await delete_key("content:kpis:summary")
            
            summary = await service.get_content_kpis(use_cache=False)
            
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.broadcast("ContentMetricsUpdate", {
                    "summary": summary.model_dump(),
                    "type": type
                })
        except Exception as e:
            logger.error(f"Erro broadcast Content: {e}")

async def setup_content_handlers():
    return ContentHubHandlers()