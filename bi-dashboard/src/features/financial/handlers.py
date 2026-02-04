import logging
from typing import Any
from datetime import datetime
import asyncio

from ...core.infrastructure.websocket import ws_manager
from ...core.infrastructure.redis_client import delete_key
from ...core.enums.hub_enums import AppHubs
from .service import create_financial_service

logger = logging.getLogger(__name__)

class FinancialHubHandlers:
    def __init__(self):
        self.feature_name = AppHubs.FINANCIAL.value

    async def handle_client_invoke(self, client_id: str, method: str, *args: Any):
        if method == "ForceUpdate":
            await self.handle_force_update(client_id)
        elif method == "GetInitialData":
            await self.handle_initial_data(client_id)
        elif method.startswith("Subscribe"):
            await self.handle_subscription(client_id, method)

    async def handle_initial_data(self, client_id: str):
        try:
            service = create_financial_service()
            task_pay = service.get_payment_summary(use_cache=True)
            task_rev = service.get_revenue_metrics(use_cache=True)
            task_cb = service.get_chargeback_summary(use_cache=True)

            pay_summary, rev_metrics, cb_summary = await asyncio.gather(task_pay, task_rev, task_cb)

            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.send_to_client(client_id, "InitialData", {
                    "paymentSummary": pay_summary.model_dump(),
                    "revenue": rev_metrics.model_dump(),
                    "chargebacks": cb_summary.model_dump(),
                    "timestamp": datetime.now().isoformat()
                })
        except Exception as e:
            logger.error(f"❌ Erro Financial InitialData: {e}")

    async def handle_force_update(self, client_id: str):
        logger.info(f"🔄 ForceUpdate Financeiro por {client_id}")
        try:
            # Reusa a lógica centralizada
            await self.broadcast_revenue_update(type="Manual")
            
            # Feedback
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                await hub.send_to_client(client_id, "UpdateProcessed", {"status": "ok"})
        except Exception as e:
            logger.error(f"❌ Erro ForceUpdate Financeiro: {e}")

    async def handle_subscription(self, client_id: str, method: str):
        hub = ws_manager.get_hub(self.feature_name)
        if hub:
            await hub.send_to_client(client_id, "SubscriptionConfirmed", {"subscription": method})

    # --- Método Público para o Loop de Background ---

    async def broadcast_full_financial_update(self, type: str = "Auto"):
        """Atualiza TUDO: Revenue, Payments e Chargebacks"""
        try:
            service = create_financial_service()
            
            # 1. Limpa Todos os Caches
            await delete_key("financial:summary:payments")
            await delete_key("financial:metrics:revenue")
            await delete_key("financial:summary:chargeback")

            # 2. Recalcula em Paralelo (Scatter)
            task_rev = service.get_revenue_metrics(use_cache=False)
            task_pay = service.get_payment_summary(use_cache=False)
            task_cb = service.get_chargeback_summary(use_cache=False)
            
            rev, pay, cb = await asyncio.gather(task_rev, task_pay, task_cb)
            
            # 3. Broadcast Unificado ou Separado
            hub = ws_manager.get_hub(self.feature_name)
            if hub:
                # Envia tudo num evento "FullUpdate" ou eventos separados
                await hub.broadcast("FinancialUpdate", {
                     "revenue": rev.model_dump(),
                     "paymentSummary": pay.model_dump(),
                     "chargebacks": cb.model_dump(),
                     "type": type
                })
        except Exception as e:
            logger.error(f"Erro broadcast Financial: {e}")

async def setup_financial_handlers():
    return FinancialHubHandlers()