import asyncio
import time
from dotenv import load_dotenv
from app.services.llm_provider import OpenAIProvider, GeminiProvider
from app.utils.logger import get_logger

logger = get_logger("LLMService")
load_dotenv()

class AllProvidersExhaustedError(Exception):
    pass

class LLMService:
    def __init__(self):
        # Initialize the list of providers in the order we want to use them
        self.providers = []
        try:
            self.providers.append(OpenAIProvider())
        except Exception as e:
            logger.warning(f"OpenAIProvider não configurado: {e}")
            
        try:
            self.providers.append(GeminiProvider())
        except Exception as e:
            logger.warning(f"GeminiProvider não configurado: {e}")

    async def enrich_product(self, product: dict) -> dict:
        prompt = f"""
        Você é um especialista em e-commerce. Traduza e reescreva a descrição abaixo 
        para português do Brasil, focando em persuasão de vendas (copywriting).
        Produto: {product.get('name', '')}
        Descrição original: {product.get('description', '')}
        
        Retorne apenas o texto da nova descrição persuasiva.
        """
        
        sku = product.get('sku')
        start_time = time.time()
        
        for provider in self.providers:
            log_extra = {"sku": sku, "provider": provider.name}
            try:
                logger.info(f"Tentando {provider.name} para SKU: {sku}...", extra=log_extra)
                
                # Só aguarda se não for o primeiro provider da lista (tentativa de fallback)
                if provider != self.providers[0]:
                    await asyncio.sleep(5)
                
                enriched_response = await provider.enrich(prompt)
                
                product['description'] = enriched_response.description
                product['status'] = "processed"
                
                duration = round(time.time() - start_time, 2)
                logger.info(f"Sucesso com {provider.name} para SKU: {sku} em {duration}s.", extra=log_extra)
                return product
                
            except Exception as e:
                # Trata erros (incluindo 429 e Network Errors) e tenta o próximo provedor
                logger.warning(f"Falha {provider.name} ({type(e).__name__}). Tentando próximo provider...", extra=log_extra)
                continue
                
        # Se esgotar a lista de provedores
        logger.error(f"Todos os provedores de LLM falharam para SKU: {sku}.", extra={"sku": sku})
        raise AllProvidersExhaustedError("Nenhum provedor de LLM disponível conseguiu processar a requisição.")