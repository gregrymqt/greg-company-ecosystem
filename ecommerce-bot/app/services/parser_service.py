import logging
from typing import Optional
from app.models.products import Product

class ParserService:
    @staticmethod
    def _extract_data(element) -> Optional[Product]:
        try:
            # Seletores baseados no HTML que você enviou
            title_tag = element.select_one("h3 > a")
            price_tag = element.select_one("p.price_color")
            link_tag = element.select_one("h3 > a")
            
            if not all([title_tag, price_tag, link_tag]):
                return None

            # O título completo está no atributo 'title'
            name = title_tag["title"]
            # O preço vem como '£51.77', removemos o £
            price_text = price_tag.text.replace("£", "").strip()
            # Usamos a URL como SKU (ou parte dela)
            sku = link_tag["href"].split("_")[-1].replace("/index.html", "")
            
            return Product(
                sku=sku,
                name=name,
                cost_price=float(price_text),
                source_url=f"https://books.toscrape.com/{link_tag['href']}",
                description="Produto extraído do site Books to Scrape."
            )
            
        except Exception as e:
            print(f"Erro ao parsear card: {e}")
            return None