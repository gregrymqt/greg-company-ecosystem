"""
FastAPI Application - BI Dashboard
API REST + WebSocket Hubs
"""

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
import logging
import asyncio

from ..core.websocket_server import setup_websocket_routes
from ..features.claims import websocket_handlers as claims_ws
from ..features.financial import websocket_handlers as financial_ws
from .routes import claims_routes, financial_routes

# Logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)

logger = logging.getLogger(__name__)


# FastAPI App
app = FastAPI(
    title="BI Dashboard API",
    description="API de Analytics com WebSocket Real-time",
    version="1.0.0"
)


# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "http://localhost:3000",
        "http://localhost:5173",
        "http://localhost:5045",
    ],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# Setup WebSocket
setup_websocket_routes(app)


# Include REST routes
app.include_router(claims_routes.router, prefix="/api/claims", tags=["Claims"])
app.include_router(financial_routes.router, prefix="/api/financial", tags=["Financial"])


# Startup event
@app.on_event("startup")
async def startup_event():
    """Inicialização do app"""
    logger.info("🚀 Iniciando BI Dashboard API...")
    
    # Setup WebSocket handlers
    await claims_ws.setup_claims_hub_handlers()
    await financial_ws.setup_financial_hub_handlers()
    
    # Inicia background tasks
    asyncio.create_task(broadcast_kpis_periodically())
    
    logger.info("✅ BI Dashboard API iniciado!")


@app.on_event("shutdown")
async def shutdown_event():
    """Shutdown do app"""
    logger.info("🛑 Encerrando BI Dashboard API...")


# Background tasks
async def broadcast_kpis_periodically():
    """
    Task em background para enviar KPIs periodicamente
    Similar ao padrão de updates automáticos do sistema principal
    """
    from ..core.infrastructure.websocket import ws_manager
    from ..features.claims import create_claims_service
    
    await asyncio.sleep(5)  # Aguarda inicialização
    
    while True:
        try:
            # Aguarda 30 segundos
            await asyncio.sleep(30)
            
            # Verifica se há clientes conectados no hub de claims
            claims_hub = ws_manager.get_hub("claims")
            if claims_hub and claims_hub.get_connection_count() > 0:
                # Busca KPIs atualizados
                service = create_claims_service()
                kpis = service.get_claims_kpis()
                
                # Broadcast
                await claims_hub.broadcast_method("KPIUpdate", {
                    "kpis": kpis,
                    "autoUpdate": True
                })
                
                logger.info(f"📊 KPIs enviados para {claims_hub.get_connection_count()} clientes")
                
        except Exception as e:
            logger.error(f"Erro no broadcast periódico de KPIs: {e}")


# Health check
@app.get("/")
async def root():
    """Health check"""
    return {
        "status": "online",
        "service": "BI Dashboard API",
        "version": "1.0.0"
    }


@app.get("/health")
async def health():
    """Health check detalhado"""
    from ..core.infrastructure.websocket import ws_manager
    
    return {
        "status": "healthy",
        "websocket": {
            "totalConnections": ws_manager.get_total_connections(),
            "hubs": ws_manager.get_hubs_status()
        }
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
