"""
Redis Client
Gerenciamento de conexão com Redis para cache e Token Blocklist
Sincronizado com o Backend C# (.NET)
"""

import os
import redis
from typing import Optional
import logging

logger = logging.getLogger(__name__)

# === CONFIGURAÇÃO ===
REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
REDIS_PORT = int(os.getenv("REDIS_PORT", "6379"))
REDIS_PASSWORD = os.getenv("REDIS_PASSWORD", None)
REDIS_DB = int(os.getenv("REDIS_DB", "0"))

# Cliente Redis global (singleton)
_redis_client: Optional[redis.Redis] = None


def get_redis_client() -> redis.Redis:
    """
    Retorna uma instância do cliente Redis (singleton)
    Cria a conexão na primeira chamada e reutiliza nas próximas
    
    Returns:
        Instância configurada do Redis
        
    Raises:
        ConnectionError: Se não conseguir conectar ao Redis
    """
    global _redis_client
    
    if _redis_client is None:
        try:
            _redis_client = redis.Redis(
                host=REDIS_HOST,
                port=REDIS_PORT,
                password=REDIS_PASSWORD,
                db=REDIS_DB,
                decode_responses=True,  # Retorna strings ao invés de bytes
                socket_connect_timeout=5,
                socket_timeout=5
            )
            
            # Testa a conexão
            _redis_client.ping()
            logger.info(f"✅ Conectado ao Redis em {REDIS_HOST}:{REDIS_PORT}")
            
        except redis.ConnectionError as e:
            logger.error(f"❌ Erro ao conectar ao Redis: {str(e)}")
            raise ConnectionError(f"Não foi possível conectar ao Redis: {str(e)}")
        except Exception as e:
            logger.error(f"❌ Erro inesperado ao configurar Redis: {str(e)}")
            raise
    
    return _redis_client


def check_token_blocklist(token: str) -> bool:
    """
    Verifica se um token JWT está na blocklist (invalidado por Logout)
    Usa o mesmo padrão de chave do Backend C#: "blacklist:{token}"
    
    Args:
        token: Token JWT a ser verificado
        
    Returns:
        True se o token está bloqueado, False caso contrário
    """
    try:
        client = get_redis_client()
        key = f"blacklist:{token}"
        
        # EXISTS retorna 1 se a chave existe, 0 caso contrário
        exists = client.exists(key)
        
        if exists:
            logger.warning(f"🚫 Token bloqueado encontrado na blacklist")
            return True
        
        return False
        
    except Exception as e:
        logger.error(f"Erro ao verificar blocklist no Redis: {str(e)}")
        # Em caso de erro de conexão, por segurança, consideramos o token como NÃO bloqueado
        # para não quebrar a aplicação se o Redis estiver temporariamente indisponível
        # Você pode mudar isso para retornar True (bloquear tudo) se preferir fail-secure
        return False


def close_redis_connection():
    """
    Fecha a conexão com o Redis
    Deve ser chamado no shutdown da aplicação
    """
    global _redis_client
    
    if _redis_client is not None:
        try:
            _redis_client.close()
            logger.info("🔌 Conexão com Redis fechada")
        except Exception as e:
            logger.error(f"Erro ao fechar conexão com Redis: {str(e)}")
        finally:
            _redis_client = None


# === FUNÇÕES AUXILIARES PARA CACHE (OPCIONAL) ===

def get_cache(key: str) -> Optional[str]:
    """
    Busca um valor no cache Redis
    
    Args:
        key: Chave do cache
        
    Returns:
        Valor armazenado ou None
    """
    try:
        client = get_redis_client()
        return client.get(key)
    except Exception as e:
        logger.error(f"Erro ao buscar cache '{key}': {str(e)}")
        return None


def set_cache(key: str, value: str, expiration_seconds: Optional[int] = None) -> bool:
    """
    Armazena um valor no cache Redis
    
    Args:
        key: Chave do cache
        value: Valor a ser armazenado
        expiration_seconds: Tempo de expiração em segundos (opcional)
        
    Returns:
        True se armazenado com sucesso, False caso contrário
    """
    try:
        client = get_redis_client()
        if expiration_seconds:
            client.setex(key, expiration_seconds, value)
        else:
            client.set(key, value)
        return True
    except Exception as e:
        logger.error(f"Erro ao armazenar cache '{key}': {str(e)}")
        return False


def delete_cache(key: str) -> bool:
    """
    Remove um valor do cache Redis
    
    Args:
        key: Chave do cache
        
    Returns:
        True se removido com sucesso, False caso contrário
    """
    try:
        client = get_redis_client()
        client.delete(key)
        return True
    except Exception as e:
        logger.error(f"Erro ao deletar cache '{key}': {str(e)}")
        return False
