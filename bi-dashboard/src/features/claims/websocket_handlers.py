"""
Claims WebSocket Handlers
Handlers para eventos e métodos invocados pelo frontend
"""

import asyncio
from typing import Optional
from datetime import datetime
import logging

from ...core.infrastructure.websocket import ws_manager, WebSocketHub
from .service import ClaimsService, create_claims_service

logger = logging.getLogger(__name__)


async def setup_claims_hub_handlers():
    """
    Configura handlers para o Claims Hub
    Chamado na inicialização do app
    """
    hub = ws_manager.get_hub("claims")
    if not hub:
        hub = ws_manager.create_hub("claims")
    
    # Registra handlers
    hub.register_handler("SubscribeToKPIUpdates", handle_subscribe_kpi_updates)
    hub.register_handler("SubscribeToNewClaims", handle_subscribe_new_claims)
    hub.register_handler("SubscribeToCriticalClaims", handle_subscribe_critical_claims)
    hub.register_handler("GetInitialData", handle_get_initial_data)
    
    logger.info("✅ Claims Hub handlers configurados")


async def handle_subscribe_kpi_updates(client_id: str, interval_seconds: int = 30):
    """
    Handler: Cliente se inscreveu para receber updates de KPIs
    Similar ao 'SubscribeToJobProgress' do sistema principal
    """
    logger.info(f"Cliente {client_id} inscrito em KPI updates (interval: {interval_seconds}s)")
    
    hub = ws_manager.get_hub("claims")
    if not hub:
        return
    
    # Envia confirmação
    await hub.send_to_client(client_id, "SubscriptionConfirmed", {
        "subscription": "KPIUpdates",
        "interval": interval_seconds
    })


async def handle_subscribe_new_claims(client_id: str):
    """Handler: Cliente quer ser notificado de novas claims"""
    logger.info(f"Cliente {client_id} inscrito em notificações de novas claims")
    
    hub = ws_manager.get_hub("claims")
    if hub:
        await hub.send_to_client(client_id, "SubscriptionConfirmed", {
            "subscription": "NewClaims"
        })


async def handle_subscribe_critical_claims(client_id: str):
    """Handler: Cliente quer ser notificado de claims críticas"""
    logger.info(f"Cliente {client_id} inscrito em claims críticas")
    
    hub = ws_manager.get_hub("claims")
    if hub:
        await hub.send_to_client(client_id, "SubscriptionConfirmed", {
            "subscription": "CriticalClaims"
        })


async def handle_get_initial_data(client_id: str):
    """
    Handler: Cliente solicita dados iniciais ao conectar
    Envia KPIs e claims ativas
    """
    logger.info(f"Cliente {client_id} solicitou dados iniciais")
    
    try:
        service = create_claims_service()
        
        # Busca KPIs
        kpis = service.get_claims_kpis()
        
        # Busca claims ativas
        active_claims = service.get_active_claims_analytics()
        
        # Envia dados
        hub = ws_manager.get_hub("claims")
        if hub:
            await hub.send_to_client(client_id, "InitialData", {
                "kpis": kpis,
                "activeClaims": [
                    {
                        "id": claim.id,
                        "mpClaimId": claim.mp_claim_id,
                        "amountAtRisk": float(claim.amount_at_risk),
                        "claimType": claim.claim_type,
                        "status": claim.internal_status,
                        "daysOpen": claim.days_open,
                        "isCritical": claim.is_critical,
                        "panelUrl": claim.panel_url
                    }
                    for claim in active_claims[:20]  # Primeiros 20
                ],
                "timestamp": datetime.now().isoformat()
            })
            
    except Exception as e:
        logger.error(f"Erro ao buscar dados iniciais: {e}")


# Funções para broadcast (chamadas pelos services)

async def broadcast_kpi_update(kpis: dict):
    """
    Envia atualização de KPIs para todos os clientes inscritos
    """
    hub = ws_manager.get_hub("claims")
    if not hub:
        return
    
    # Envia para todos que se inscreveram
    await hub.broadcast_method("KPIUpdate", {
        "kpis": kpis,
        "timestamp": datetime.now().isoformat()
    })


async def broadcast_new_claim(claim_data: dict):
    """
    Notifica nova claim para todos os clientes
    """
    hub = ws_manager.get_hub("claims")
    if not hub:
        return
    
    await hub.broadcast_method("NewClaimNotification", {
        "claim": claim_data,
        "timestamp": datetime.now().isoformat()
    })


async def broadcast_critical_claim_alert(claim_data: dict):
    """
    Alerta de claim crítica
    """
    hub = ws_manager.get_hub("claims")
    if not hub:
        return
    
    await hub.broadcast_method("CriticalClaimAlert", {
        "claim": claim_data,
        "severity": "high",
        "timestamp": datetime.now().isoformat()
    })
