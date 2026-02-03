import logging
from typing import Any
from datetime import datetime
import asyncio

from ...core.infrastructure.websocket import ws_manager
from ...core.infrastructure.redis_client import delete_key
from ...core.enums.hub_enums import AppHubs
from .service import create_claims_service

logger = logging.getLogger(__name__)

class ClaimsHubHandlers:
    def __init__(self):
        self.feature_name = AppHubs.CLAIMS.value

    async def handle_client_invoke(self, client_id: str, method: str, *args: Any):
        """
        Roteador central para Claims.
        O 'main.py' ou 'websocket_router.py' chamará este método.
        """
        if method == "ForceUpdate":
            await self.handle_force_update(client_id)
        elif method == "GetInitialData":
            await self.handle_initial_data(client_id)
        elif method.startswith("Subscribe"):
            await self.handle_subscription(client_id, method)

    async def handle_initial_data(self, client_id: str):
        try:
            service = create_claims_service()
            
            # Busca paralela (Cache + Banco)
            task_kpis = service.get_claims_kpis(use_cache=True)
            task_active = service.get_active_claims_analytics()
            
            kpis, active_claims = await asyncio.gather(task_kpis, task_active)
            
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.send_to_client(client_id, "InitialData", {
                    "kpis": kpis,
                    "activeClaims": [
                        {
                            "id": c.id,
                            "amountAtRisk": float(c.amount_at_risk),
                            "status": c.internal_status,
                            "isCritical": c.is_critical,
                            "daysOpen": c.days_open
                        } for c in active_claims[:20]
                    ],
                    "timestamp": datetime.now().isoformat()
                })
        except Exception as e:
            logger.error(f"❌ Erro InitialData Claims: {e}")

    async def handle_force_update(self, client_id: str):
        """Ação do usuário (Botão Atualizar)"""
        logger.info(f"🔄 ForceUpdate Claims por {client_id}")
        try:
            # 1. Executa a atualização e broadcast
            await self.broadcast_claims_update(type="Manual")
            
            # 2. Feedback específico para quem clicou
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.send_to_client(client_id, "UpdateProcessed", {"status": "ok"})
                
        except Exception as e:
            logger.error(f"❌ Erro ForceUpdate Claims: {e}")

    async def handle_subscription(self, client_id: str, method: str):
        """Trata inscrições específicas"""
        hub = ws_manager.get_hub(self.feature_name)
        if hub:
            # Ex: SubscribeToCriticalClaims -> Confirmed
            await hub.send_to_client(client_id, "SubscriptionConfirmed", {"topic": method})

    # --- Método para o Loop do Main.py (Sem Client ID) ---
    
    async def broadcast_claims_update(self, type: str = "Auto"):
        """
        Limpa cache, recalcula e envia para TODOS.
        Usado pelo Loop Infinito e pelo ForceUpdate.
        """
        try:
            service = create_claims_service()
            
            # 1. Limpa Cache
            await delete_key("claims:kpis:dashboard")
            
            # 2. Recalcula (Bypass Cache)
            updated_kpis = await service.get_claims_kpis(use_cache=False)
            
            # 3. Broadcast
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.broadcast("KPIUpdate", {
                    "kpis": updated_kpis,
                    "type": type,
                    "timestamp": datetime.now().isoformat()
                })
        except Exception as e:
            logger.error(f"Erro broadcast Claims: {e}")

# Factory para o Main.py
async def setup_claims_handlers():
    return ClaimsHubHandlers()