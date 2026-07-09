import os
import asyncio
from datetime import datetime, timezone
import logging
from dotenv import load_dotenv

from app.config.database import db
from app.models.products import Product, ProductStatus
from app.services.csv_export_service import CsvExportService

load_dotenv()
logger = logging.getLogger(__name__)

class ExporterWorker:
    """
    Worker assíncrono responsável por buscar produtos enriquecidos e integrá-los
    com o novo motor CsvExportService.
    """
    def __init__(self, tenant_id: str, platform="shopify"):
        self.tenant_id = tenant_id
        self.platform = platform.lower()

    async def fetch_processed_products(self):
        """Busca no MongoDB os produtos que já passaram pela IA e estão com status 'processed'."""
        try:
            collection = db.client["ecommerce"]["products"]
            query = {"status": ProductStatus.PROCESSED.value, "tenant_id": self.tenant_id}
            raw_products = await collection.find(query).to_list(length=5000)
            products = []
            for rp in raw_products:
                try:
                    products.append(Product(**rp))
                except Exception as e:
                    logger.warning(f"Aviso: Não foi possível carregar o produto {rp.get('_id')} no modelo Product. Erro: {e}")
            return products
        except Exception as e:
            logger.error(f"Erro ao buscar produtos no MongoDB: {e}")
            return []

    async def export(self):
        """Executa o fluxo completo: Buscar, delegar pro Service, salvar e atualizar Banco."""
        logger.info(f"Iniciando processo de exportação para {self.platform.capitalize()}...")
        
        products = await self.fetch_processed_products()
        
        if not products:
            logger.info("Nenhum produto com status 'processed' encontrado. Nada a exportar.")
            return

        logger.info(f"Encontrados {len(products)} produtos para exportar.")

        # Preparar dados para o Export Service (convertendo do Modelo Pydantic)
        product_dicts = []
        for p in products:
            p_dict = p.model_dump()
            # Mapeia atributos caso a LLM tenha gerado tags e campos SEO dentro do dict 'attributes'
            p_dict["tags"] = p_dict.get("attributes", {}).get("tags", [])
            p_dict["seo_title"] = p_dict.get("attributes", {}).get("seo_title", p.title)
            p_dict["seo_description"] = p_dict.get("attributes", {}).get("seo_description", p.description[:150])
            product_dicts.append(p_dict)

        # Delegação para o CsvExportService
        csv_bytes = b""
        if self.platform == "shopify":
            csv_bytes = CsvExportService.generate_shopify_csv(product_dicts)
        elif self.platform == "nuvemshop":
            csv_bytes = CsvExportService.generate_nuvemshop_csv(product_dicts)
        else:
            logger.warning(f"Plataforma '{self.platform}' não configurada no ExporterWorker.")
            return

        # Simula a gravação de um arquivo localmente por agora (ou pode ser repassado via API)
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        filename = f"export_{self.platform}_{timestamp}.csv"

        try:
            with open(filename, mode='wb') as file:
                file.write(csv_bytes)
            
            logger.info(f"Sucesso: Arquivo gerado em '{filename}'.")
            
            # Etapa crucial: marcar no banco como exportado para evitar duplicidade
            await self.mark_as_exported([p.sku for p in products])
            
        except Exception as e:
            logger.error(f"Erro crítico ao gerar o arquivo CSV: {e}")

    async def mark_as_exported(self, product_skus):
        """Atualiza os documentos exportados para 'status: Exported' no MongoDB."""
        if not product_skus:
            return

        try:
            collection = db.client["ecommerce"]["products"]
            result = await collection.update_many(
                {"tenant_id": self.tenant_id, "sku": {"$in": product_skus}},
                {"$set": {"status": ProductStatus.EXPORTED.value, "updated_at": datetime.now(timezone.utc)}}
            )
            logger.info(f"Concluído: {result.modified_count} documentos marcados como 'Exported' no MongoDB.")
        except Exception as e:
            logger.error(f"Erro ao atualizar o status no MongoDB: {e}")

if __name__ == "__main__":
    async def main():
        from app.config.database import connect_to_mongo, close_mongo_connection
        await connect_to_mongo()
        
        # Teste rápido
        exporter = ExporterWorker(tenant_id="default_tenant", platform="shopify")
        await exporter.export()
        
        await close_mongo_connection()
        
    asyncio.run(main())
