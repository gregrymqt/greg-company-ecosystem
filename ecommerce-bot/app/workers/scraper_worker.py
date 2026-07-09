import httpx
import asyncio
import logging
from bs4 import BeautifulSoup
from app.models.products import Product, ScraperMetadata, ProductStatus
from app.services.json_ld_parser_service import JsonLdParserService
from app.services.markdown_parser_service import MarkdownParserService
from app.services.notification_service import NotificationService
from app.config.database import db
from urllib.parse import urljoin, urlparse
from datetime import datetime, timezone, timedelta
import os
from dotenv import load_dotenv

load_dotenv()

class ScraperWorker:
    def __init__(self, repository):
        self.client = httpx.AsyncClient(timeout=10.0, follow_redirects=True)
        self.repository = repository
        self.json_ld_parser = JsonLdParserService()
        
        # Load API key for MarkdownParserService
        api_key = os.getenv("OPENAI_API_KEY", "")
        self.markdown_parser = MarkdownParserService(api_key=api_key)
        self.notification_service = NotificationService()

    async def _handle_scraping_failure(self, domain: str, error_type: str, url: str):
        """Registra falha consecutiva e dispara alerta se atingir o limiar (Regra dos 3 Erros)."""
        collection = db.client["ecommerce"]["scraping_metadata"]
        
        doc = await collection.find_one_and_update(
            {"domain": domain},
            {"$inc": {"consecutive_failures": 1}},
            upsert=True,
            return_document=True
        )
        
        failures = doc.get("consecutive_failures", 1)
        silenced_until = doc.get("silenced_until")
        
        # Verifica Anti-Spam (cool-down)
        is_silenced = silenced_until and silenced_until.replace(tzinfo=timezone.utc) > datetime.now(timezone.utc)
        
        if failures >= 3 and not is_silenced:
            # Dispara webhook
            await self.notification_service.send_discord_alert(domain, error_type, url)
            
            # Silencia novos alertas para esse domínio por 1 hora
            await collection.update_one(
                {"domain": domain},
                {"$set": {"silenced_until": datetime.now(timezone.utc) + timedelta(hours=1)}}
            )

    async def _handle_scraping_success(self, domain: str):
        """Reseta o contador de falhas após um scraping bem sucedido."""
        collection = db.client["ecommerce"]["scraping_metadata"]
        await collection.update_one(
            {"domain": domain},
            {"$set": {"consecutive_failures": 0}}
        )

    async def _process_product_page(self, product_url: str):
        """Busca e extrai os dados do produto individual."""
        domain = urlparse(product_url).netloc
        error_type = "Parser retornou dados nulos"

        try:
            # Tenta a Estratégia 1
            product_dict = await self.json_ld_parser.parse(product_url)
            
            # Se falhou em trazer o básico (title ou description nulos), tenta o Fallback (Estratégia 2)
            if not product_dict.get("title") or not product_dict.get("description"):
                logging.info(f"Estratégia 1 falhou para {product_url}. Acionando Fallback LLM.")
                # Precisamos do HTML para a estratégia 2
                response = await self.client.get(product_url)
                response.raise_for_status()
                product_dict = await self.markdown_parser.parse(response.text)
                
            if not product_dict.get("title"):
                logging.warning(f"Não foi possível extrair dados estruturados de {product_url}")
                await self._handle_scraping_failure(domain, error_type, product_url)
                return None
                
            # Preenche o modelo
            product_obj = Product(
                sku=product_dict.get("sku") or product_url.split("/")[-2],
                title=product_dict.get("title"),
                description=product_dict.get("description") or "Descrição não encontrada",
                price=float(product_dict.get("price") or 0.0),
                currency=product_dict.get("currency") or "BRL",
                images=[product_dict.get("image_url")] if product_dict.get("image_url") else [],
                metadata=ScraperMetadata(source_url=product_url),
                status=ProductStatus.RAW
            )
            
            # Resetamos os erros pois tivemos sucesso
            await self._handle_scraping_success(domain)
            
            return product_obj
            
        except Exception as e:
            logging.error(f"Erro ao processar página de produto {product_url}: {e}")
            await self._handle_scraping_failure(domain, str(e), product_url)
            return None

    async def run(self, base_url: str):
        # A primeira página é a própria base_url
        current_url = base_url
        
        while current_url:
            logging.info(f"Scraping catálogo: {current_url}")
            
            try:
                response = await self.client.get(current_url)
                if response.status_code != 200:
                    break
                
                soup = BeautifulSoup(response.text, 'html.parser')
                
                # 1. Encontrar links de produtos na página atual
                # (Adaptado para o formato do books.toscrape.com ou genérico)
                products_elements = soup.select("article.product_pod h3 a")
                product_links = [urljoin(current_url, el["href"]) for el in products_elements]
                
                for product_url in product_links:
                    product_obj = await self._process_product_page(product_url)
                    if product_obj:
                        await self.repository.upsert_product(product_obj)
                    await asyncio.sleep(1) # Pausa entre produtos
                
                # 2. Procurar o link "next"
                next_button = soup.select_one("li.next > a")
                if next_button:
                    current_url = urljoin(current_url, next_button["href"])
                else:
                    logging.info("Fim da paginação.")
                    current_url = None
                
                await asyncio.sleep(2)
                
            except Exception as e:
                logging.error(f"Erro na navegação do catálogo: {e}")
                break