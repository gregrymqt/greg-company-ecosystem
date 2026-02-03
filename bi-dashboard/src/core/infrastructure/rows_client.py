import os
import httpx
import logging
from typing import Optional
from ...features.rows.schemas import RowsUpdateRequest

logger = logging.getLogger(__name__)

class RowsClient:
    _instance: Optional['RowsClient'] = None
    _http_client: Optional[httpx.AsyncClient] = None

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance

    def __init__(self):
        # Evita re-inicializar variáveis se já for singleton
        if not hasattr(self, 'initialized'):
            self.api_key = os.getenv("ROWS_API_KEY")
            self.spreadsheet_id = os.getenv("ROWS_SPREADSHEET_ID")
            self.table_id = os.getenv("ROWS_TABLE_ID")
            self.base_url = "https://api.rows.com/v1"
            self.initialized = True
            
            if not all([self.api_key, self.spreadsheet_id, self.table_id]):
                logger.error("❌ Faltam variáveis de ambiente do ROWS")

    async def get_http_client(self) -> httpx.AsyncClient:
        """Retorna o cliente HTTP persistente ou cria um novo"""
        if self._http_client is None or self._http_client.is_closed:
            self._http_client = httpx.AsyncClient(
                base_url=self.base_url,
                headers={
                    "Authorization": f"Bearer {self.api_key}",
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                timeout=30.0 # Timeout maior para envio de listas grandes
            )
            logger.info("🔌 Cliente HTTP Rows iniciado")
        return self._http_client

    async def send_data(self, range_address: str, data: RowsUpdateRequest):
        """
        Envia dados para o Rows reutilizando a conexão.
        """
        client = await self.get_http_client()
        url = f"/spreadsheets/{self.spreadsheet_id}/tables/{self.table_id}/cells/{range_address}"
        
        # Converte para JSON compatível
        payload_json = data.model_dump(mode='json')

        try:
            # Não precisamos de 'with client' aqui, pois ele é persistente
            response = await client.post(url, json=payload_json)
            response.raise_for_status()
            logger.debug(f"✅ [Rows] Enviado para {range_address}")
            return response.json()
        except httpx.HTTPStatusError as e:
            logger.error(f"❌ Erro HTTP Rows ({e.response.status_code}): {e.response.text}")
            raise e
        except Exception as e:
            logger.error(f"❌ Erro conexão Rows: {str(e)}")
            raise e

    async def close(self):
        """Fecha a conexão ao desligar a API"""
        if self._http_client and not self._http_client.is_closed:
            await self._http_client.aclose()
            logger.info("🔌 Conexão Rows fechada")

rows_client = RowsClient()