import logging
from app.config.database import db
from app.models.products import Product

class ProductRepository:
    async def upsert_product(self, product: Product):
        try:
            collection = db.client["ecommerce"]["products"]
            product_data = product.model_dump(by_alias=True)
            result = await collection.update_one(
                {"sku": product.sku}, 
                {"$set": product_data}, 
                upsert=True
            )
            return result.upserted_id or result.modified_count > 0
        except Exception as e:
            logging.error(f"Erro ao persistir SKU {product.sku}: {e}")
            raise # Repassa o erro se você quiser que o Worker saiba que falhou