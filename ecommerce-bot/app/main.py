import asyncio
import logging
from contextlib import asynccontextmanager
from fastapi import FastAPI
import uvicorn

from app.config.settings import settings
from app.config.database import connect_to_mongo, close_mongo_connection
from app.repositories.product_repository import ProductRepository
from app.workers.scraper_worker import ScraperWorker
from app.api.v1.endpoints import router as v1_router

logger = logging.getLogger(__name__)

# Configuração global de logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup
    logger.info("Iniciando aplicação e conectando aos serviços...")
    await connect_to_mongo()
    
    repository = ProductRepository()
    scraper_worker = ScraperWorker(repository)
    
    # Inicia a task em background para o worker não bloquear o servidor HTTP
    worker_task = asyncio.create_task(scraper_worker.start_consuming("ecommerce_prod"))
    app.state.worker_task = worker_task

    yield
    
    # Shutdown
    logger.info("Encerrando aplicação e fechando conexões...")
    worker_task.cancel()
    try:
        await worker_task
    except asyncio.CancelledError:
        pass
    await close_mongo_connection()

app = FastAPI(title="Ecommerce Bot API", lifespan=lifespan)

app.include_router(v1_router, prefix="/api/v1")

if __name__ == "__main__":
    uvicorn.run("app.main:app", host="0.0.0.0", port=8000, reload=True)