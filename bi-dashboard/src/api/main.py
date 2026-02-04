import logging
import asyncio
from contextlib import asynccontextmanager
from fastapi import FastAPI, Depends
from fastapi.middleware.cors import CORSMiddleware

# Infraestrutura Core
from src.core.infrastructure.redis_client import get_redis_client, close_redis_connection
from src.core.infrastructure.database import db_connection
from src.core.infrastructure.mongo_client import mongo_connection
from src.core.infrastructure.rows_client import rows_client
from src.core.infrastructure.websocket import ws_manager
from src.core.security import get_current_user
from src.core.enums.hub_enums import AppHubs
from src.core.websocket_router import setup_websocket_routes

# Importação dos Handlers (Ajuste os nomes dos arquivos se necessário)
# Estou assumindo que seus arquivos chamam 'handlers.py' ou 'websocket_handlers.py'
from src.features.claims import handlers as claims_ws
from src.features.financial import handlers as financial_ws
from src.features.support import handlers as support_ws
from src.features.subscriptions import handlers as subscriptions_ws
from src.features.users import handlers as users_ws
from src.features.content import handlers as content_ws
from src.features.storage import handlers as storage_ws

# Importação das Rotas REST
from src.features.claims import routes as claims_routes
from src.features.financial import routes as financial_routes
from src.features.support import routes as support_routes
from src.features.subscriptions import routes as subscriptions_routes
from src.features.users import routes as users_routes
from src.features.content import routes as content_routes
from src.features.storage import routes as storage_routes

# Logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# ==============================================================================
# 1. LIFESPAN (REGISTRO DOS HUBS)
# ==============================================================================

@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("🚀 Iniciando Greg Company BI API (2.0)...")
    
    # A. Teste de Conexões de Infra
    try:
        await get_redis_client()
        await db_connection.test_connection() # Teste SQL
        await mongo_connection.test_connection() # Teste MongoDB
        await mongo_connection.create_indexes()
        logger.info("✅ Infraestrutura (Redis/SQL/MongoDB) conectada.")
    except Exception as e:
        logger.critical(f"⚠️ Falha na Infraestrutura: {str(e)}")
    
    # B. Registro dos WebSocket Hubs (O Pulo do Gato)
    # Aqui conectamos a lógica de cada Feature ao Gerente de Sockets
    try:
        # 1. Claims
        claims_handler = await claims_ws.setup_claims_handlers()
        ws_manager.create_hub(AppHubs.CLAIMS.value).register_handler(claims_handler.handle_client_invoke)

        # 2. Financial
        fin_handler = await financial_ws.setup_financial_handlers()
        ws_manager.create_hub(AppHubs.FINANCIAL.value).register_handler(fin_handler.handle_client_invoke)

        # 3. Support
        support_handler = await support_ws.setup_support_handlers()
        ws_manager.create_hub(AppHubs.SUPPORT.value).register_handler(support_handler.handle_client_invoke)

        # 4. Subscriptions
        sub_handler = await subscriptions_ws.setup_subscriptions_handlers()
        ws_manager.create_hub(AppHubs.SUBSCRIPTIONS.value).register_handler(sub_handler.handle_client_invoke)

        # 5. Users
        users_handler = await users_ws.setup_users_handlers()
        ws_manager.create_hub(AppHubs.USERS.value).register_handler(users_handler.handle_client_invoke)

        # 6. Content
        content_handler = await content_ws.setup_content_handlers()
        ws_manager.create_hub(AppHubs.CONTENT.value).register_handler(content_handler.handle_client_invoke)

        # 7. Storage
        storage_handler = await storage_ws.setup_storage_handlers()
        ws_manager.create_hub(AppHubs.STORAGE.value).register_handler(storage_handler.handle_client_invoke)
        
        logger.info("✅ Todos os 7 WebSocket Hubs foram registrados com sucesso.")
        
    except Exception as e:
        logger.error(f"❌ Erro fatal ao registrar Hubs: {e}")
    
    # C. Inicia Tarefas em Background (Broadcast Automático)
    broadcast_task = asyncio.create_task(broadcast_kpis_periodically())
    
    yield
    
    # --- SHUTDOWN ---
    logger.info("🛑 Encerrando serviços...")
    broadcast_task.cancel() # Para o loop infinito
    await rows_client.close()
    await db_connection.close()
    await close_redis_connection()
    await mongo_connection.close()

# ==============================================================================
# 2. APP SETUP
# ==============================================================================

app = FastAPI(
    title="Greg Company BI API",
    version="2.0.0",
    lifespan=lifespan
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "http://localhost:3000",
        "http://localhost:5173",
        "http://127.0.0.1:5173",  # Frontend React (desenvolvimento)
        "http://localhost:5045",   # Backend C# (caso o frontend passe por proxy)
        "http://127.0.0.1:5045",
    ],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Registra rota /ws
setup_websocket_routes(app)

# ==============================================================================
# 3. ROTAS REST (Com Auth)
# ==============================================================================

api_prefix = "/api"

app.include_router(claims_routes.router, prefix=f"{api_prefix}/claims", tags=["Claims"], dependencies=[Depends(get_current_user)])
app.include_router(financial_routes.router, prefix=f"{api_prefix}/financial", tags=["Financial"], dependencies=[Depends(get_current_user)])
app.include_router(storage_routes.router, prefix=f"{api_prefix}/storage", tags=["Storage"], dependencies=[Depends(get_current_user)])
app.include_router(support_routes.router, prefix=f"{api_prefix}/support", tags=["Support"], dependencies=[Depends(get_current_user)])
app.include_router(subscriptions_routes.router, prefix=f"{api_prefix}/subscriptions", tags=["Subscriptions"], dependencies=[Depends(get_current_user)])
app.include_router(users_routes.router, prefix=f"{api_prefix}/users", tags=["Users"], dependencies=[Depends(get_current_user)])
app.include_router(content_routes.router, prefix=f"{api_prefix}/content", tags=["Content"], dependencies=[Depends(get_current_user)])

# ==============================================================================
# 4. BACKGROUND TASKS
# ==============================================================================

async def broadcast_kpis_periodically():
    """Loop infinito que envia atualizações automáticas para os dashboards"""
    logger.info("⏳ Iniciando Broadcast Loop...")
    await asyncio.sleep(10) # Aguarda warmup inicial
    
    while True:
        try:
            tasks = [
                claims_ws.ClaimsHubHandlers().broadcast_claims_update(type="Auto"),
                financial_ws.FinancialHubHandlers().broadcast_full_financial_update(type="Auto"),
                content_ws.ContentHubHandlers().broadcast_content_update(type="Auto"),
                support_ws.SupportHubHandlers().broadcast_support_update(type="Auto"),
                storage_ws.StorageHubHandlers().broadcast_storage_update(type="Auto"),
                subscriptions_ws.SubscriptionsHubHandlers().broadcast_subscriptions_update(type="Auto"),
                users_ws.UsersHubHandlers().broadcast_users_update(type="Auto")
            ]
            
            # Executa todos em paralelo sem travar se um falhar
            await asyncio.gather(*tasks, return_exceptions=True)
            
        except Exception as e:
            logger.error(f"Erro no broadcast periódico: {e}")
            
        await asyncio.sleep(60) # Roda a cada 60 segundos

# ==============================================================================
# 5. HEALTH CHECK
# ==============================================================================

@app.get("/health")
async def health():
    return {
        "status": "online",
        "connections": ws_manager.get_total_connections(),
        "hubs_active": ws_manager.get_hubs_status()
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)