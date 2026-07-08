import os
import json
import logging
import asyncio
import httpx
from bs4 import BeautifulSoup
import pika
from dotenv import load_dotenv

from app.services.parser_service import ParserService
from app.services.llm_service import LLMService
from app.services.rabbitmq_service import rabbitmq_service

load_dotenv()
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger("worker_main")

async def process_message(ch, method, properties, body):
    payload = json.loads(body.decode("utf-8"))
    product_id = payload.get("ProductId")
    tenant_id = payload.get("TenantId")
    target_url = payload.get("TargetUrl")
    
    logger.info(f"Received import request for ProductId: {product_id}, TargetUrl: {target_url}")
    
    try:
        # 1. Scrape the TargetUrl
        async with httpx.AsyncClient(timeout=15.0) as client:
            response = await client.get(target_url)
            if response.status_code != 200:
                raise Exception(f"Failed to fetch {target_url}, status code {response.status_code}")
            
            soup = BeautifulSoup(response.text, 'html.parser')
            # Extract using existing ParserService or fallback
            product_element = soup.select_one("article.product_pod")
            if product_element:
                product_data = ParserService._extract_data(product_element)
            else:
                product_data = None
            
            if not product_data:
                # Fallback to simple extraction if parser fails for the specific URL layout
                title = soup.select_one("h1").text if soup.select_one("h1") else "Unknown Product"
                price_tag = soup.select_one("p.price_color")
                price = price_tag.text if price_tag else "0.0"
                
                desc_tag = soup.select_one("#product_description ~ p")
                description = desc_tag.text if desc_tag else "Produto genérico extraído"
                
                product_data = {
                    "name": title.strip(),
                    "description": description.strip(),
                    "price": float(price.replace("£", "").replace("€", "").replace("$", "").strip()),
                    "sku": target_url.split("/")[-2] if "/" in target_url else ""
                }

        # 2. Use LLM
        llm = LLMService()
        enriched_product = await llm.enrich_product(product_data)

        # 3. Mount payload
        result_payload = {
            "Success": True,
            "ProductId": product_id,
            "TenantId": tenant_id,
            "Title": enriched_product.get("name"),
            "Description": enriched_product.get("description"),
            "Price": enriched_product.get("price") or enriched_product.get("cost_price", 0.0),
            "Images": enriched_product.get("images", []),
            "Attributes": enriched_product.get("attributes", {})
        }
        
    except Exception as e:
        logger.error(f"Error processing {product_id}: {e}")
        result_payload = {
            "Success": False,
            "ProductId": product_id,
            "TenantId": tenant_id,
            "Error": str(e)
        }

    # 4. Publish result
    rabbitmq_service.publish_completed_event(result_payload)
    
    # 5. Ack
    ch.basic_ack(delivery_tag=method.delivery_tag)
    logger.info(f"Processed and ACKed message for ProductId: {product_id}")

def process_message_sync(ch, method, properties, body):
    asyncio.run(process_message(ch, method, properties, body))

def start_consuming():
    host = os.getenv("RABBITMQ_HOST", "localhost")
    user = os.getenv("RABBITMQ_USER", "guest")
    password = os.getenv("RABBITMQ_PASS", "guest")
    port = int(os.getenv("RABBITMQ_PORT", "5672"))
    
    credentials = pika.PlainCredentials(user, password)
    parameters = pika.ConnectionParameters(host=host, port=port, credentials=credentials, heartbeat=60)
    
    connection = pika.BlockingConnection(parameters)
    channel = connection.channel()
    
    queue_name = "product.import.request"
    channel.queue_declare(queue=queue_name, durable=True)
    
    channel.basic_qos(prefetch_count=1)
    channel.basic_consume(queue=queue_name, on_message_callback=process_message_sync, auto_ack=False)
    
    logger.info(f"Worker is waiting for messages in {queue_name}. To exit press CTRL+C")
    try:
        channel.start_consuming()
    except KeyboardInterrupt:
        logger.info("Interrupted. Stopping consumer...")
        channel.stop_consuming()
    finally:
        connection.close()

if __name__ == "__main__":
    start_consuming()