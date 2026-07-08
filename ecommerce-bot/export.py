import os
import csv
from datetime import datetime
from pymongo import MongoClient
from dotenv import load_dotenv
import sys

# Garante que as importações absolutas do app funcionem a partir da raiz
sys.path.append(os.path.dirname(os.path.abspath(__file__)))
from app.models.products import Product

# Carrega variáveis de ambiente (como MONGO_URI)
load_dotenv()

MONGO_URI = os.getenv("MONGO_URI", "mongodb://localhost:27017")
DATABASE_NAME = os.getenv("MONGO_DB_NAME", "ecommerce_db")
COLLECTION_NAME = os.getenv("MONGO_COLLECTION_NAME", "products")

class ExporterWorker:
    """
    Worker responsável por exportar produtos enriquecidos para plataformas de E-commerce.
    """
    def __init__(self, platform="shopify"):
        self.client = MongoClient(MONGO_URI)
        self.db = self.client[DATABASE_NAME]
        self.collection = self.db[COLLECTION_NAME]
        self.platform = platform.lower()

    def fetch_processed_products(self):
        """Busca no MongoDB os produtos que já passaram pela IA e estão com status 'processed'."""
        try:
            # Traz apenas produtos 'Processed'
            raw_products = list(self.collection.find({"status": "Processed"}))
            products = []
            for rp in raw_products:
                try:
                    products.append(Product(**rp))
                except Exception as e:
                    print(f"Aviso: Não foi possível carregar o produto {rp.get('_id')} no modelo Product. Erro: {e}")
            return products
        except Exception as e:
            print(f"Erro ao buscar produtos no MongoDB: {e}")
            return []

    def map_to_shopify(self, product: Product):
        """
        Mapeia o modelo Product para o padrão de colunas do Shopify.
        """
        image_url = str(product.images[0]) if product.images else ""
        if not image_url:
            print(f"Aviso: Produto {product.sku} não possui imagem.")

        return {
            "Handle": product.sku.lower().replace(" ", "-"), # Identificador amigável na URL
            "Title": product.title,
            "Body (HTML)": product.description, # Aqui entra a descrição rica gerada pela IA
            "Vendor": "Loja Padrão",
            "Type": product.category,
            "Tags": "",
            "Published": "TRUE",
            "Option1 Name": "Title",
            "Option1 Value": "Default Title",
            "Variant SKU": product.sku,
            "Variant Grams": 0,
            "Variant Price": product.price,
            "Image Src": str(product.images[0]) if product.images else ""
        }

    def map_to_nuvemshop(self, product: Product):
        """
        Exemplo extra: Mapeamento para Nuvemshop.
        """
        return {
            "Identificador URL": product.sku,
            "Nome": product.title,
            "Categorias": product.category,
            "Preço": product.price,
            "Descrição": product.description,
            "Imagens": ",".join([str(img) for img in product.images]) if product.images else ""
        }

    def export(self):
        """Executa o fluxo completo: Buscar, Mapear, Exportar para CSV e Atualizar Banco."""
        print(f"[{datetime.now()}] Iniciando processo de exportação para {self.platform.capitalize()}...")
        
        products = self.fetch_processed_products()
        
        if not products:
            print("Nenhum produto com status 'processed' encontrado. Nada a exportar.")
            return

        print(f"Encontrados {len(products)} produtos para exportar.")

        # Direciona para o mapeamento correto da plataforma alvo
        mapped_data = []
        if self.platform == "shopify":
            from app.models.products import ScraperMetadata
            dummy_product = Product(sku="dummy", title="dummy", description="dummy", price=1.0, metadata=ScraperMetadata(source_url="http://dummy.com"))
            fieldnames = self.map_to_shopify(dummy_product).keys()
            mapped_data = [self.map_to_shopify(p) for p in products]
        elif self.platform == "nuvemshop":
            from app.models.products import ScraperMetadata
            dummy_product = Product(sku="dummy", title="dummy", description="dummy", price=1.0, metadata=ScraperMetadata(source_url="http://dummy.com"))
            fieldnames = self.map_to_nuvemshop(dummy_product).keys()
            mapped_data = [self.map_to_nuvemshop(p) for p in products]
        else:
            print(f"Plataforma '{self.platform}' não configurada no ExporterWorker.")
            return

        # Gera o arquivo CSV
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        filename = f"export_{self.platform}_{timestamp}.csv"

        try:
            with open(filename, mode='w', newline='', encoding='utf-8') as file:
                writer = csv.DictWriter(file, fieldnames=fieldnames)
                writer.writeheader()
                writer.writerows(mapped_data)
            
            print(f"Sucesso: Arquivo gerado em '{filename}'.")
            
            # Etapa crucial: marcar no banco como exportado para evitar duplicidade
            self.mark_as_exported([p.id for p in products if p.id])
            
        except Exception as e:
            print(f"Erro crítico ao gerar o arquivo CSV: {e}")

    def mark_as_exported(self, product_ids):
        """Atualiza os documentos exportados para 'status: exported' no MongoDB."""
        if not product_ids:
            return

        try:
            result = self.collection.update_many(
                {"_id": {"$in": product_ids}},
                {"$set": {"status": "Exported"}}
            )
            print(f"Concluído: {result.modified_count} documentos marcados como 'Exported' no MongoDB.")
        except Exception as e:
            print(f"Erro ao atualizar o status no MongoDB: {e}")

if __name__ == "__main__":
    # Exemplo de como inicializar e rodar o worker para o Shopify
    exporter = ExporterWorker(platform="shopify")
    exporter.export()
    
    # Se fosse para Nuvemshop:
    # exporter_nuvem = ExporterWorker(platform="nuvemshop")
    # exporter_nuvem.export()
