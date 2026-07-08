import os
import csv
from datetime import datetime
from pymongo import MongoClient
from dotenv import load_dotenv

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
            # Traz apenas produtos 'processed'
            return list(self.collection.find({"status": "processed"}))
        except Exception as e:
            print(f"Erro ao buscar produtos no MongoDB: {e}")
            return []

    def map_to_shopify(self, product):
        """
        Mapeia o documento do MongoDB para o padrão de colunas do Shopify.
        Muitos campos são fixos ou dependem da regra de negócio.
        """
        # Tratamento de segurança para dados ausentes, como a URL da imagem
        image_url = product.get("image_url", "")
        if not image_url:
            print(f"Aviso: Produto {product.get('sku')} não possui imagem.")

        return {
            "Handle": str(product.get("sku", "")).lower().replace(" ", "-"), # Identificador amigável na URL
            "Title": product.get("title", "Produto sem título"),
            "Body (HTML)": product.get("description", ""), # Aqui entra a descrição rica gerada pela IA
            "Vendor": product.get("vendor", "Loja Padrão"),
            "Type": product.get("category", "Geral"),
            "Tags": ",".join(product.get("tags", [])),
            "Published": "TRUE",
            "Option1 Name": "Title",
            "Option1 Value": "Default Title",
            "Variant SKU": product.get("sku", ""),
            "Variant Grams": product.get("weight", 0),
            "Variant Inventory Tracker": "shopify",
            "Variant Inventory Qty": product.get("stock", 0),
            "Variant Inventory Policy": "deny",
            "Variant Fulfillment Service": "manual",
            "Variant Price": product.get("price", 0.0),
            "Variant Compare At Price": product.get("compare_at_price", ""),
            "Variant Requires Shipping": "TRUE",
            "Variant Taxable": "TRUE",
            "Image Src": image_url,
            "Image Position": 1,
            "Image Alt Text": product.get("title", ""),
        }

    def map_to_nuvemshop(self, product):
        """
        Exemplo extra: Mapeamento para Nuvemshop.
        """
        image_url = product.get("image_url", "")
        return {
            "Identificador URL": str(product.get("sku", "")).lower().replace(" ", "-"),
            "Nome": product.get("title", ""),
            "Descrição": product.get("description", ""),
            "Categorias": product.get("category", "Geral"),
            "Preço": product.get("price", 0.0),
            "Preço Promocional": product.get("compare_at_price", ""),
            "Estoque": product.get("stock", 0),
            "SKU": product.get("sku", ""),
            "Exibir na loja": "Sim",
            "Imagens": image_url
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
            mapped_data = [self.map_to_shopify(p) for p in products]
            fieldnames = self.map_to_shopify({}).keys()
        elif self.platform == "nuvemshop":
            mapped_data = [self.map_to_nuvemshop(p) for p in products]
            fieldnames = self.map_to_nuvemshop({}).keys()
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
            self.mark_as_exported([p["_id"] for p in products])
            
        except Exception as e:
            print(f"Erro crítico ao gerar o arquivo CSV: {e}")

    def mark_as_exported(self, product_ids):
        """Atualiza os documentos exportados para 'status: exported' no MongoDB."""
        if not product_ids:
            return

        try:
            result = self.collection.update_many(
                {"_id": {"$in": product_ids}},
                {"$set": {"status": "exported"}}
            )
            print(f"Concluído: {result.modified_count} documentos marcados como 'exported' no MongoDB.")
        except Exception as e:
            print(f"Erro ao atualizar o status no MongoDB: {e}")

if __name__ == "__main__":
    # Exemplo de como inicializar e rodar o worker para o Shopify
    exporter = ExporterWorker(platform="shopify")
    exporter.export()
    
    # Se fosse para Nuvemshop:
    # exporter_nuvem = ExporterWorker(platform="nuvemshop")
    # exporter_nuvem.export()
