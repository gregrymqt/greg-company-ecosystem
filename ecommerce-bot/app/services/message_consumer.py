import os
import json
import logging
import asyncio
import hashlib
import httpx
from motor.motor_asyncio import AsyncIOMotorClient
from bs4 import BeautifulSoup
import pika
from dotenv import load_dotenv

from app.services.parser_service import ParserService
from app.services.llm_service import LLMService
from app.services.rabbitmq_service import rabbitmq_service

from app.models.products import Product
from app.models.messages import ImportRequestMessage, ImportCompletedMessage

load_dotenv()
logger = logging.getLogger("worker_consumer")

async def process_message(ch, method, properties, body):
    try:
        raw_payload = json.loads(body.decode("utf-8"))
        
        # Inject TenantId from headers if present, prioritizing headers
        if properties.headers and (properties.headers.get("TenantId") or properties.headers.get("tenant_id")):
            raw_payload["TenantId"] = properties.headers.get("TenantId") or properties.headers.get("tenant_id")
            
        request_msg = ImportRequestMessage.model_validate(raw_payload)
    except Exception as e:
        logger.error(f"Error parsing incoming message: {e}")
        ch.basic_ack(delivery_tag=method.delivery_tag)
        return

    product_id = request_msg.product_id
    tenant_id = request_msg.tenant_id
    target_url = request_msg.target_url
    logger.info(f"Received import request for ProductId: {product_id}, TargetUrl: {target_url}")
    
    mongo_uri = os.getenv('MONGO_URI', "mongodb://localhost:27017")
    db_client = AsyncIOMotorClient(mongo_uri)
    
    try:
        # Check Tenant Saldo
        tenant_col = db_client["ecommerce"]["tenants"]
        tenant_doc = await tenant_col.find_one({"tenant_id": tenant_id})
        
        if not tenant_doc or tenant_doc.get("saldo", 0) <= 0:
            logger.warning(f"Tenant {tenant_id} sem saldo ou não encontrado. Ignorando processamento.")
            ch.basic_ack(delivery_tag=method.delivery_tag)
            return

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
                
                from app.models.products import ScraperMetadata
                product_data = Product(
                    title=title.strip(),
                    description=description.strip(),
                    price=float(price.replace("£", "").replace("€", "").replace("$", "").strip()),
                    sku=target_url.split("/")[-2] if "/" in target_url else "",
                    metadata=ScraperMetadata(source_url=target_url)
                )

        # 2. Use LLM com Lookup de Cache Semântico
        product_hash = hashlib.sha256(product_data.model_dump_json(exclude={"status", "created_at"}).encode("utf-8")).hexdigest()
        cache_col = db_client["ecommerce"]["llm_cache"]
        cached_doc = await cache_col.find_one({"hash": product_hash})
        
        if cached_doc:
            logger.info(f"Cache hit para o produto {product_id} (hash {product_hash}). Reutilizando dados semânticos.")
            enriched_product = Product.model_validate(cached_doc["enriched_product"])
        else:
            logger.info(f"Cache miss para o produto {product_id}. Iniciando chamada para o LLM...")
            
            # Fetch custom API key if BYOK is configured
            tenant_openai_key = tenant_doc.get("settings", {}).get("openai_api_key") or tenant_doc.get("openai_api_key")
            llm = LLMService(openai_api_key=tenant_openai_key)
            
            enriched_product = await llm.enrich_product(product_data)
            
            await cache_col.insert_one({
                "hash": product_hash,
                "enriched_product": enriched_product.model_dump(mode="json")
            })

        # 3. Mount payload
        result_payload = ImportCompletedMessage(
            Success=True,
            ProductId=product_id,
            TenantId=tenant_id,
            Title=enriched_product.title,
            Description=enriched_product.description,
            Price=enriched_product.price,
            Images=[str(img) for img in enriched_product.images] if enriched_product.images else []
        )
        
    except Exception as e:
        logger.error(f"Error processing {product_id}: {e}")
        result_payload = ImportCompletedMessage(
            Success=False,
            ProductId=product_id,
            TenantId=tenant_id,
            Error=str(e)
        )

    finally:
        db_client.close()

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
