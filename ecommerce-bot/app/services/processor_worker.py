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
    async def _process_with_retry(self, product, current_llm=None):
        llm_to_use = current_llm or self.llm
        return await llm_to_use.enrich_product(product)

    async def run(self):
        logger.info("ProcessorWorker iniciado. Monitorando novos produtos...")
        while True:
            collection = db.client["ecommerce"]["products"]
            
            # Limpeza de produtos órfãos (travados em Processing há mais de 10 min)
            ten_minutes_ago = datetime.now(timezone.utc) - timedelta(minutes=10)
            await collection.update_many(
                {"status": "Processing", "updated_at": {"$lt": ten_minutes_ago}},
                {"$set": {"status": "Raw"}}
            )

            # Pega um produto com status Raw e atualiza para Processing atomicamente
            product = await collection.find_one_and_update(
                {"status": "Raw"},
                {"$set": {"status": "Processing", "updated_at": datetime.now(timezone.utc)}},
                return_document=True
            )
            
            if not product:
                # Se não houver nada, dorme antes de checar de novo
                await asyncio.sleep(10)
                continue

            from app.models.products import Product
            try:
                product_model = Product(**product)
            except Exception as e:
                logger.error(f"Erro de validação no DB para o produto {product.get('sku', 'unknown')}: {e}", exc_info=True)
                await collection.update_one(
                    {"_id": product["_id"]},
                    {"$set": {"status": "Failed", "last_error": str(e), "updated_at": datetime.now(timezone.utc)}}
                )
                continue

            sku = product_model.sku
            log_extra = {"sku": sku}
            try:
                logger.info(f"Iniciando SKU: {sku}", extra=log_extra)
                
                # Suporte a BYOK (Bring Your Own Key) para o ProcessorWorker
                current_llm = self.llm
                if product_model.tenant_id:
                    tenant_col = collection.database["tenants"]
                    tenant_doc = await tenant_col.find_one({"tenant_id": product_model.tenant_id})
                    if tenant_doc:
                        tenant_openai_key = tenant_doc.get("settings", {}).get("openai_api_key") or tenant_doc.get("openai_api_key")
                        if tenant_openai_key:
                            from app.services.llm_service import LLMService
                            logger.info(f"Usando chave de API própria (BYOK) para o tenant: {product_model.tenant_id}")
                            current_llm = LLMService(openai_api_key=tenant_openai_key)
                            
                processed_data = await self._process_with_retry(product_model, current_llm)
                await self.repo.upsert_product(processed_data)
            except (ValueError, TypeError) as e:
                logger.error(f"Erro de validação de dados no produto {sku}: {e}", extra=log_extra, exc_info=True)
                await collection.update_one(
                    {"sku": sku},
                    {"$set": {"status": "Failed", "last_error": str(e), "updated_at": datetime.now(timezone.utc)}}
                )
            except Exception as e:
                logger.error(f"Erro final (após retries) ao processar produto {sku}: {e}", extra=log_extra, exc_info=True)
                # Atualiza para erro para não entrar em loop infinito (Poison Pill)
                await collection.update_one(
                    {"sku": sku},
                    {"$set": {"status": "Failed", "last_error": str(e), "updated_at": datetime.now(timezone.utc)}}
                )
            
            # Pequena pausa entre itens para não sobrecarregar as APIs
            await asyncio.sleep(1)