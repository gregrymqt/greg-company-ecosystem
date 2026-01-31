"""
Financial WebSocket Handlers
"""

import logging
from datetime import datetime
from ...core.infrastructure.websocket import ws_manager
from .service import create_financial_service

logger = logging.getLogger(__name__)


async def setup_financial_hub_handlers():
    """Configura handlers para o Financial Hub"""
    hub = ws_manager.get_hub("financial")
    if not hub:
        hub = ws_manager.create_hub("financial")
    
    hub.register_handler("SubscribeToRevenueUpdates", handle_subscribe_revenue_updates)
    hub.register_handler("SubscribeToNewPayments", handle_subscribe_new_payments)
    hub.register_handler("GetInitialData", handle_get_initial_data)
    
    logger.info("✅ Financial Hub handlers configurados")


async def handle_subscribe_revenue_updates(client_id: str):
    """Handler: Cliente inscrito em updates de receita"""
    logger.info(f"Cliente {client_id} inscrito em Revenue Updates")
    
    hub = ws_manager.get_hub("financial")
    if hub:
        await hub.send_to_client(client_id, "SubscriptionConfirmed", {
            "subscription": "RevenueUpdates"
        })


async def handle_subscribe_new_payments(client_id: str):
    """Handler: Cliente inscrito em novos pagamentos"""
    logger.info(f"Cliente {client_id} inscrito em New Payments")
    
    hub = ws_manager.get_hub("financial")
    if hub:
        await hub.send_to_client(client_id, "SubscriptionConfirmed", {
            "subscription": "NewPayments"
        })


async def handle_get_initial_data(client_id: str):
    """Handler: Envia dados iniciais financeiros"""
    try:
        service = create_financial_service()
        summary = service.get_payment_summary()
        
        hub = ws_manager.get_hub("financial")
        if hub:
            await hub.send_to_client(client_id, "InitialData", {
                "paymentSummary": {
                    "totalPayments": summary.TotalPayments,
                    "totalApproved": float(summary.TotalApproved),
                    "approvalRate": summary.ApprovalRate
                },
                "timestamp": datetime.now().isoformat()
            })
    except Exception as e:
        logger.error(f"Erro ao buscar dados iniciais financeiros: {e}")


async def broadcast_revenue_update(revenue_data: dict):
    """Broadcast de atualização de receita"""
    hub = ws_manager.get_hub("financial")
    if hub:
        await hub.broadcast_method("RevenueUpdate", {
            "revenue": revenue_data,
            "timestamp": datetime.now().isoformat()
        })
