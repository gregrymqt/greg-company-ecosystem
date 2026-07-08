import httpx
import asyncio
import logging
from bs4 import BeautifulSoup

class ScraperWorker:
    def __init__(self, repository, parser):
        self.client = httpx.AsyncClient(timeout=10.0)
        self.repository = repository
        self.parser = parser

    async def run(self, base_url: str):
        # A primeira página é a própria base_url
        current_url = base_url
        
        while current_url:
            logging.info(f"Scraping: {current_url}")
            
            try:
                response = await self.client.get(current_url)
                if response.status_code != 200:
                    break
                
                soup = BeautifulSoup(response.text, 'html.parser')
                
                # 1. Processar produtos da página atual
                products = soup.find_all("article", class_="product_pod")
                for product_element in products:
                    product_obj = self.parser._extract_data(product_element)
                    if product_obj:
                        await self.repository.upsert_product(product_obj)
                
                # 2. Procurar o link "next"
                next_button = soup.select_one("li.next > a")
                if next_button:
                    # O link no site é relativo (ex: 'catalogue/page-2.html')
                    # Precisamos completar a URL com a base do site
                    next_page_path = next_button["href"]
                    current_url = f"https://books.toscrape.com/catalogue/{next_page_path}"
                else:
                    # Não tem mais botão next, encerramos
                    logging.info("Fim da paginação.")
                    current_url = None
                
                await asyncio.sleep(2)
                
            except Exception as e:
                logging.error(f"Erro na navegação: {e}")
                break