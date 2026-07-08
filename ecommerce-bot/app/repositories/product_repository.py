import logging
from app.database import db

class ProductRepository:
    async def upsert_product(self, product_data: dict):
        try:
            collection = db.client["ecommerce"]["products"]
            result = await collection.update_one(
                {"sku": product_data["sku"]}, 
                {"$set": product_data}, 
                upsert=True
            )
            return result.upserted_id or result.modified_count > 0
        except Exception as e:
            logging.error(f"Erro ao persistir SKU {product_data.get('sku')}: {e}")
            raise # Repassa o erro se você quiser que o Worker saiba que falhou