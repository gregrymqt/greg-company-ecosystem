import asyncio
import logging
import os
from app.services.scraper_worker import ScraperWorker
from app.services.processor_worker import ProcessorWorker
from app.repositories.product_repository import ProductRepository
from app.services.parser_service import ParserService
from app.services.llm_service import LLMService
from app.database import connect_to_mongo, close_mongo_connection
from dotenv import load_dotenv

# Carrega variáveis do .env
load_dotenv()

# Configuração de logs com formatação melhorada
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)

async def main():
    try:
        # 1. Iniciar conexão com banco
        await connect_to_mongo()
        
        # Instanciar as dependências compartilhadas
        repository = ProductRepository()
        parser = ParserService()
        llm = LLMService()
        
        # Instanciar seus workers com injeção de dependência
        scraper = ScraperWorker(repository=repository, parser=parser)
        processor = ProcessorWorker(repo=repository, llm=llm)
        
        logging.info("Iniciando pipeline: Scraper + Processor")
        
        # 2. Rodar ambos simultaneamente
        base_url = os.getenv("SCRAPER_BASE_URL", "https://site-do-fornecedor.com/produtos")
        await asyncio.gather(
            scraper.run(base_url=base_url),
            processor.run()
        )
        
    except Exception as e:
        logging.error(f"Erro fatal no pipeline: {e}")
    finally:
        # 3. Fechar conexão ao finalizar
        await close_mongo_connection()

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        logging.info("Aplicação encerrada pelo usuário.")