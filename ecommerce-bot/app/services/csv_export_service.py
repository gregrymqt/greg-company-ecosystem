import csv
import io
import re
from typing import List, Dict

class CsvExportService:
    """
    Serviço dedicado à geração de arquivos CSV de produtos compatíveis com plataformas de E-commerce.
    """
    
    @staticmethod
    def _create_slug(title: str) -> str:
        """Transforma um título em um slug (URL friendly)."""
        if not title:
            return ""
        # Remove caracteres especiais e troca espaços por hífens
        slug = re.sub(r'[^\w\s-]', '', title.lower())
        return re.sub(r'[\s_-]+', '-', slug).strip('-')

    @staticmethod
    def generate_shopify_csv(products: List[Dict]) -> bytes:
        """
        Gera o payload em bytes de um CSV formatado para o Shopify.
        """
        headers = [
            "Title", "URL handle", "Description", "Tags", "Status",
            "SKU", "Price", "SEO title", "SEO description", "Published on online store"
        ]
        
        output = io.StringIO()
        # O Shopify aceita delimitador padrão ',' e escape por aspas.
        writer = csv.DictWriter(output, fieldnames=headers, quoting=csv.QUOTE_MINIMAL)
        writer.writeheader()
        
        for p in products:
            tags = p.get("tags", "")
            tags_str = ",".join(tags) if isinstance(tags, list) else str(tags)
            
            title = p.get("title", "")
            
            row = {
                "Title": title,
                "URL handle": CsvExportService._create_slug(title),
                "Description": p.get("description", ""),
                "Tags": tags_str,
                "Status": "draft",
                "SKU": p.get("sku", ""),
                "Price": p.get("price", 0.0),
                "SEO title": p.get("seo_title", title),
                "SEO description": p.get("seo_description", ""),
                "Published on online store": "TRUE"
            }
            writer.writerow(row)
            
        # O formato utf-8-sig (com BOM) é vital para o Excel carregar a codificação corretamente
        return output.getvalue().encode('utf-8-sig')

    @staticmethod
    def generate_nuvemshop_csv(products: List[Dict]) -> bytes:
        """
        Gera o payload em bytes de um CSV formatado para a Nuvemshop.
        """
        headers = [
            "Identificador URL", "Nome", "Preço", "Descrição", "Tags",
            "Título para SEO", "Descrição para SEO", "Exibir na loja", "Produto Físico"
        ]
        
        output = io.StringIO()
        # Nuvemshop utiliza comumente o delimitador ';'
        writer = csv.DictWriter(output, fieldnames=headers, delimiter=';', quoting=csv.QUOTE_MINIMAL)
        writer.writeheader()
        
        for p in products:
            tags = p.get("tags", "")
            tags_str = ",".join(tags) if isinstance(tags, list) else str(tags)
            title = p.get("title", "")
            
            row = {
                "Identificador URL": CsvExportService._create_slug(title),
                "Nome": title,
                "Preço": p.get("price", 0.0),
                "Descrição": p.get("description", ""),
                "Tags": tags_str,
                "Título para SEO": p.get("seo_title", title),
                "Descrição para SEO": p.get("seo_description", ""),
                "Exibir na loja": "SIM",
                "Produto Físico": "SIM"
            }
            writer.writerow(row)
            
        return output.getvalue().encode('utf-8-sig')
