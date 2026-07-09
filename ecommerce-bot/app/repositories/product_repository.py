import logging
from app.config.database import db
from app.models.products import Product

class ProductRepository:
    async def upsert_product(self, product: Product) -> bool:
        try:
            collection = db.client["ecommerce"]["products"]
            
            # Converte o modelo para dicionário respeitando os aliases (_id)
            product_data = product.model_dump(by_alias=True)
            
            # RECORTE CRÍTICO: Remove o _id do payload de atualização.
            # O Mongo não permite modificar ou reenviar o _id em operações de $set se o doc já existir.
            product_data.pop("_id", None)
            
            # CORREÇÃO MULTI-TENANT: A busca DEVE considerar o tenant_id + sku combinados!
            result = await collection.update_one(
                {
                    "tenant_id": product.tenant_id, 
                    "sku": product.sku
                }, 
                {"$set": product_data}, 
                upsert=True
            )
            
            return result.upserted_id is not None or result.modified_count > 0
            
        except Exception as e:
            logging.error(f"Erro ao persistir SKU {product.sku} para o Tenant {product.tenant_id}: {e}")
            raise