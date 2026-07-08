import asyncio
import logging
from datetime import datetime, timezone, timedelta
from tenacity import retry, wait_exponential, stop_after_attempt, retry_if_exception_type
from app.database import db
from app.services.llm_service import AllProvidersExhaustedError
from app.utils.logger import get_logger

logger = get_logger("ProcessorWorker")

def _log_retry(retry_state):
    logger.warning(f"Retentando processamento LLM... Tentativa {retry_state.attempt_number}")

class ProcessorWorker:
    def __init__(self, repo, llm):
        self.repo = repo
        self.llm = llm

    @retry(
        wait=wait_exponential(multiplier=1, min=4, max=60), 
        stop=stop_after_attempt(5),
        retry=retry_if_exception_type(AllProvidersExhaustedError),
        before_sleep=_log_retry
    )
    async def _process_with_retry(self, product):
        return await self.llm.enrich_product(product)

    async def run(self):
        logger.info("ProcessorWorker iniciado. Monitorando novos produtos...")
        while True:
            collection = db.client["ecommerce"]["products"]
            
            # Limpeza de produtos órfãos (travados em processing há mais de 10 min)
            ten_minutes_ago = datetime.now(timezone.utc) - timedelta(minutes=10)
            await collection.update_many(
                {"status": "processing", "updated_at": {"$lt": ten_minutes_ago}},
                {"$set": {"status": "raw"}}
            )

            # Pega um produto com status raw e atualiza para processing atomicamente
            product = await collection.find_one_and_update(
                {"status": "raw"},
                {"$set": {"status": "processing", "updated_at": datetime.now(timezone.utc)}},
                return_document=True
            )
            
            if not product:
                # Se não houver nada, dorme antes de checar de novo
                await asyncio.sleep(10)
                continue

            sku = product.get('sku')
            log_extra = {"sku": sku}
            try:
                logger.info(f"Iniciando SKU: {sku}", extra=log_extra)
                processed_data = await self._process_with_retry(product)
                await self.repo.upsert_product(processed_data)
            except (ValueError, TypeError) as e:
                logger.error(f"Erro de validação de dados no produto {sku}: {e}", extra=log_extra, exc_info=True)
                await collection.update_one(
                    {"sku": sku},
                    {"$set": {"status": "validation_error", "last_error": str(e), "updated_at": datetime.now(timezone.utc)}}
                )
            except Exception as e:
                logger.error(f"Erro final (após retries) ao processar produto {sku}: {e}", extra=log_extra, exc_info=True)
                # Atualiza para erro para não entrar em loop infinito (Poison Pill)
                await collection.update_one(
                    {"sku": sku},
                    {"$set": {"status": "error", "last_error": str(e), "updated_at": datetime.now(timezone.utc)}}
                )
            
            # Pequena pausa entre itens para não sobrecarregar as APIs
            await asyncio.sleep(1)