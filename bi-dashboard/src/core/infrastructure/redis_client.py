"""
Redis Infrastructure Module (ASYNC)
Stack: FastAPI + redis.asyncio
Features: Caching Pattern (GetOrCreate), Token Blocklist, Auto-Serialization
"""

import os
import json
import logging
from typing import Optional, Any, Callable, TypeVar
import redis.asyncio as redis # Driver Async oficial
from fastapi.encoders import jsonable_encoder

logger = logging.getLogger(__name__)

# Configurações de Ambiente
REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
REDIS_PORT = int(os.getenv("REDIS_PORT", "6379"))
REDIS_PASSWORD = os.getenv("REDIS_PASSWORD", None)
REDIS_DB = int(os.getenv("REDIS_DB", "0"))

# Singleton do Cliente
_redis_client: Optional[redis.Redis] = None

# Generics para tipagem do retorno
T = TypeVar("T")

async def get_redis_client() -> redis.Redis:
    """
    Retorna a instância singleton do Redis Client (Async).
    Gerencia conexão e reconexão automática.
    """
    global _redis_client
    
    if _redis_client is None:
        try:
            _redis_client = redis.Redis(
                host=REDIS_HOST,
                port=REDIS_PORT,
                password=REDIS_PASSWORD,
                db=REDIS_DB,
                decode_responses=True, # Importante: Retorna str, não bytes
                socket_connect_timeout=5,
                socket_timeout=5
            )
            await _redis_client.ping()
            logger.info(f"✅ Conectado ao Redis (Async) em {REDIS_HOST}:{REDIS_PORT}")
            
        except Exception as e:
            logger.error(f"❌ Falha crítica no Redis: {str(e)}")
            raise
    
    return _redis_client

# ==============================================================================
#  Feature 1: Smart Caching (Get or Create)
# ==============================================================================

async def get_or_create_async(
    key: str, 
    factory: Callable[[], Any], 
    ttl_seconds: int = 300
) -> Any:
    """
    Tenta buscar no cache. Se falhar (Miss), executa a função 'factory' (ex: query SQL),
    salva o resultado no Redis e retorna.
    
    :param key: Chave única do cache
    :param factory: Função ASSÍNCRONA que busca o dado original (se cache falhar)
    :param ttl_seconds: Tempo de vida em segundos (Padrão: 5 min)
    """
    client = await get_redis_client()
    
    try:
        # 1. Tenta pegar do cache
        cached_data = await client.get(key)
        
        if cached_data:
            logger.debug(f"⚡ Cache HIT: {key}")
            return json.loads(cached_data)
            
    except Exception as e:
        logger.warning(f"⚠️ Erro ao ler cache (ignorando): {e}")

    # 2. Se não achou (Cache MISS), executa a função original
    logger.debug(f"🐢 Cache MISS: {key} -> Executando Factory")
    fresh_data = await factory()
    
    if fresh_data:
        try:
            # 3. Salva no cache para a próxima vez
            # jsonable_encoder garante que objetos Pydantic/Dates virem JSON válido
            serialized = json.dumps(jsonable_encoder(fresh_data))
            await client.set(key, serialized, ex=ttl_seconds)
        except Exception as e:
            logger.error(f"❌ Erro ao salvar no cache: {e}")
            
    return fresh_data

async def delete_key(key: str):
    """Remove uma chave específica (ex: limpar cache ao atualizar dados)"""
    try:
        client = await get_redis_client()
        await client.delete(key)
        logger.info(f"🗑️ Cache invalidado: {key}")
    except Exception as e:
        logger.error(f"Erro ao deletar chave Redis: {e}")

# ==============================================================================
#  Feature 2: Token Security (Blocklist/Logout)
# ==============================================================================

async def add_token_to_blocklist(token: str, ttl_seconds: int = 3600):
    """
    Invalida um token JWT (Logout).
    O TTL deve ser igual ao tempo restante de vida do token.
    """
    try:
        client = await get_redis_client()
        key = f"blacklist:{token}"
        # Valor "1" é arbitrário, o importante é a existência da chave
        await client.set(key, "1", ex=ttl_seconds)
        logger.info(f"🚫 Token adicionado à blocklist (TTL: {ttl_seconds}s)")
    except Exception as e:
        logger.error(f"Erro ao bloquear token: {e}")

async def check_token_blocklist(token: str) -> bool:
    """Verifica se o token foi invalidado/bloqueado"""
    try:
        client = await get_redis_client()
        key = f"blacklist:{token}"
        exists = await client.exists(key)
        return exists > 0
    except Exception as e:
        logger.error(f"Erro ao verificar blocklist: {e}")
        # Em caso de erro no Redis, por segurança, deixamos passar ou bloqueamos?
        # Geralmente fail-open (False) para não parar a API, mas depende da criticidade.
        return False

# ==============================================================================
#  Lifecycle
# ==============================================================================

async def close_redis_connection():
    global _redis_client
    if _redis_client is not None:
        await _redis_client.close()
        logger.info("🔌 Conexão Redis encerrada")
        _redis_client = None