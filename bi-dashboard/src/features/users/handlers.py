import logging
from typing import Any
from ...core.infrastructure.websocket import ws_manager
from ...core.infrastructure.redis_client import delete_key
from ...core.enums.hub_enums import AppHubs
from .service import create_users_service

logger = logging.getLogger(__name__)

class UsersHubHandlers:
    def __init__(self):
        self.feature_name = AppHubs.USERS.value

    async def handle_client_invoke(self, client_id: str, method: str, *args: Any):
        if method == "ForceUpdate":
            await self.handle_force_update(client_id)
        elif method == "GetInitialData":
            await self.handle_initial_data(client_id)

    async def handle_initial_data(self, client_id: str):
        try:
            service = create_users_service()
            summary = await service.get_user_summary(use_cache=True)
            
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.send_to_client(client_id, "UserSummaryUpdate", {
                    "summary": summary.model_dump(),
                    "type": "Initial"
                })
        except Exception as e:
            logger.error(f"❌ Erro Users InitialData: {e}")

    async def handle_force_update(self, client_id: str):
        logger.info(f"🔄 ForceUpdate Users por {client_id}")
        try:
            # 1. Chama a lógica centralizada (que o Loop também usa)
            await self.broadcast_users_update(type="Manual")
            
            # 2. Feedback para quem clicou
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.send_to_client(client_id, "UpdateProcessed", {"status": "ok"})
        except Exception as e:
            logger.error(f"❌ Erro ForceUpdate Users: {e}")

    # --- Método Público para o Loop de Background ---

    async def broadcast_users_update(self, type: str = "Auto"):
        """Limpa cache, recalcula e faz broadcast (Sem Client ID)"""
        try:
            service = create_users_service()
            
            # Limpa Cache
            await delete_key("users:kpis:summary")
            
            # Recalcula
            summary = await service.get_user_summary(use_cache=False)
            
            # Broadcast
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.broadcast("UserSummaryUpdate", {
                    "summary": summary.model_dump(),
                    "type": type
                })
        except Exception as e:
            logger.error(f"Erro broadcast Users: {e}")

async def setup_users_handlers():
    return UsersHubHandlers()