import httpx
import logging
from typing import List, Dict, Any, Optional
from app.models.nuvemshop_models import NuvemshopProductRequest

logger = logging.getLogger(__name__)

class NuvemshopService:
    """
    Serviço responsável pela integração direta com a API REST da Nuvemshop (Tiendanube).
    Abstrai o CRUD e operações de lote mapeadas para o ecossistema.
    """
    
    def __init__(self, store_id: str, access_token: str, app_email: str = "suporte@gregcompany.com"):
        self.store_id = store_id
        self.access_token = access_token
        self.base_url = f"https://api.nuvemshop.com.br/v1/{self.store_id}"
        
        # Headers mandatórios extraídos da documentação de ferramentas da Nuvemshop
        self.headers = {
            "Authorization": f"Bearer {self.access_token}",
            "Content-Type": "application/json",
            "User-Agent": f"EcommerceBotGreg ({app_email})"
        }

    async def create_product(self, product: NuvemshopProductRequest) -> Dict[str, Any]:
        """
        [POST] /v1/{store_id}/products
        Cria um novo produto na Nuvemshop usando a model validada pelo Pydantic.
        """
        url = f"{self.base_url}/products"
        # Usamos by_alias=True para garantir que chaves como 'handle' sejam enviadas corretamente
        payload = product.model_dump(by_alias=True, exclude_none=True)
        
        async with httpx.AsyncClient() as client:
            try:
                response = await client.post(url, headers=self.headers, json=payload)
                response.raise_for_status() # Levanta exceção para status 4xx ou 5xx
                logger.info(f"Produto criado com sucesso na Nuvemshop. ID: {response.json().get('id')}")
                return response.json()
            except httpx.HTTPStatusError as e:
                logger.error(f"Erro ao criar produto na Nuvemshop [Status {e.response.status_code}]: {e.response.text}")
                raise e

    async def get_product_by_id(self, product_id: int) -> Dict[str, Any]:
        """
        [GET] /v1/{store_id}/products/{id}
        Busca um único produto pelo ID numérico nativo.
        """
        url = f"{self.base_url}/products/{product_id}"
        
        async with httpx.AsyncClient() as client:
            try:
                response = await client.get(url, headers=self.headers)
                response.raise_for_status()
                return response.json()
            except httpx.HTTPStatusError as e:
                logger.error(f"Erro ao buscar produto {product_id} na Nuvemshop: {e.response.text}")
                raise e

    async def get_product_by_sku(self, sku: str) -> Optional[Dict[str, Any]]:
        """
        [GET] /v1/{store_id}/products/sku/{sku}
        Busca o primeiro produto correspondente ao SKU informado.
        Devolve None caso o produto não seja encontrado (404).
        """
        url = f"{self.base_url}/products/sku/{sku}"
        
        async with httpx.AsyncClient() as client:
            try:
                response = await client.get(url, headers=self.headers)
                if response.status_code == 404:
                    logger.warning(f"Produto com SKU {sku} não encontrado na Nuvemshop.")
                    return None
                response.raise_for_status()
                return response.json()
            except httpx.HTTPStatusError as e:
                logger.error(f"Erro ao buscar SKU {sku} na Nuvemshop: {e.response.text}")
                raise e

    async def update_product_metadata(self, product_id: int, update_data: Dict[str, Any]) -> Dict[str, Any]:
        """
        [PUT] /v1/{store_id}/products/{id}
        Modifica metadados estruturais de um produto (ex: nome, descrição HTML da IA).
        ⚠️ Atenção: Para preço/estoque em produtos sem variantes explícitas, 
        a Nuvemshop exige alteração na variante virtual.
        """
        url = f"{self.base_url}/products/{product_id}"
        
        async with httpx.AsyncClient() as client:
            try:
                response = await client.put(url, headers=self.headers, json=update_data)
                response.raise_for_status()
                logger.info(f"Metadados do produto {product_id} atualizados com sucesso.")
                return response.json()
            except httpx.HTTPStatusError as e:
                logger.error(f"Erro ao atualizar produto {product_id}: {e.response.text}")
                raise e

    async def update_stock_price_batch(self, batch_data: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """
        [PATCH] /v1/{store_id}/products/stock-price
        Atualização massiva de estoque e preço de até 50 variantes simultâneas.
        Essencial para o motor de sincronização automática em tempo real.
        """
        url = f"{self.base_url}/products/stock-price"
        
        if len(batch_data) > 50:
            raise ValueError("A API da Nuvemshop permite o limite máximo de 50 variantes por lote no PATCH.")

        async with httpx.AsyncClient() as client:
            try:
                response = await client.patch(url, headers=self.headers, json=batch_data)
                response.raise_for_status()
                logger.info("Atualização em lote de preço/estoque processada.")
                return response.json()
            except httpx.HTTPStatusError as e:
                logger.error(f"Erro no PATCH em lote da Nuvemshop: {e.response.text}")
                raise e

    async def delete_product(self, product_id: int) -> bool:
        """
        [DELETE] /v1/{store_id}/products/{id}
        Remove um produto da base da loja virtual do cliente.
        """
        url = f"{self.base_url}/products/{product_id}"
        
        async with httpx.AsyncClient() as client:
            try:
                response = await client.delete(url, headers=self.headers)
                response.raise_for_status()
                logger.info(f"Produto {product_id} removido com sucesso da Nuvemshop.")
                return True
            except httpx.HTTPStatusError as e:
                logger.error(f"Erro ao deletar produto {product_id}: {e.response.text}")
                raise e