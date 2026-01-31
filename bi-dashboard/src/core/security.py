"""
Security Module - JWT Authentication
Valida tokens JWT gerados pelo Backend C# (.NET)
Compartilha a mesma Secret Key para garantir interoperabilidade
Verifica Token Blocklist no Redis para respeitar Logout do C#
"""

import os
from typing import Optional, Dict, Any
from datetime import datetime
from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from jose import JWTError, jwt
import logging
from .infrastructure.redis_client import check_token_blocklist

logger = logging.getLogger(__name__)

# === CONFIGURAÇÃO ===
# A Secret Key DEVE ser a mesma usada pelo Backend C# (.NET)
# Ler de variável de ambiente para garantir sincronia
JWT_SECRET_KEY = os.getenv("JWT_SECRET", "")
JWT_ALGORITHM = os.getenv("JWT_ALGORITHM", "HS256")

if not JWT_SECRET_KEY:
    logger.warning("⚠️  JWT_SECRET não configurado! Autenticação pode falhar.")

# Security scheme para extração do token do header Authorization: Bearer <token>
oauth2_scheme = HTTPBearer()


# === FUNÇÕES DE VALIDAÇÃO ===

def decode_jwt_token(token: str) -> Optional[Dict[str, Any]]:
    """
    Decodifica e valida um token JWT
    
    Args:
        token: Token JWT a ser validado
        
    Returns:
        Payload decodificado se válido, None se inválido
    """
    try:
        # Decodifica o token usando a mesma chave do C#
        payload = jwt.decode(
            token,
            JWT_SECRET_KEY,
            algorithms=[JWT_ALGORITHM]
        )
        
        # Verifica expiração (exp claim)
        exp = payload.get("exp")
        if exp:
            exp_datetime = datetime.fromtimestamp(exp)
            if exp_datetime < datetime.now():
                logger.warning("Token expirado")
                return None
        
        return payload
        
    except JWTError as e:
        logger.error(f"Erro ao decodificar JWT: {str(e)}")
        return None
    except Exception as e:
        logger.error(f"Erro inesperado na validação do token: {str(e)}")
        return None


async def get_current_user(
    credentials: HTTPAuthorizationCredentials = Depends(oauth2_scheme)
) -> Dict[str, Any]:
    """
    Dependência FastAPI para validar o usuário autenticado
    Extrai o token do header Authorization e valida sua assinatura
    Verifica se o token foi revogado (Logout) consultando a Blocklist no Redis
    
    Args:
        credentials: Credenciais extraídas do header Authorization
        
    Returns:
        Payload do usuário decodificado do token
        
    Raises:
        HTTPException 401: Se token inválido, expirado, ausente ou revogado
    """
    token = credentials.credentials
    
    if not token:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token de autenticação não fornecido",
            headers={"WWW-Authenticate": "Bearer"},
        )
    
    # ETAPA 1: Valida a assinatura do JWT
    payload = decode_jwt_token(token)
    
    if payload is None:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token inválido ou expirado",
            headers={"WWW-Authenticate": "Bearer"},
        )
    
    # ETAPA 2: Verifica se o token está na Blocklist do Redis (Logout)
    # Usa o mesmo padrão do C#: "blacklist:{token}"
    if check_token_blocklist(token):
        logger.warning("Token revogado (usuário fez logout)")
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token revogado. Faça login novamente.",
            headers={"WWW-Authenticate": "Bearer"},
        )
    
    # Log para debug (não mostrar em produção)
    user_id = payload.get("sub") or payload.get("nameid") or payload.get("id")
    logger.info(f"Usuário autenticado: {user_id}")
    
    return payload


async def get_optional_user(
    credentials: Optional[HTTPAuthorizationCredentials] = Depends(HTTPBearer(auto_error=False))
) -> Optional[Dict[str, Any]]:
    """
    Dependência FastAPI para validação OPCIONAL de usuário
    Útil para endpoints que podem funcionar com ou sem autenticação
    
    Args:
        credentials: Credenciais opcionais do header Authorization
        
    Returns:
        Payload do usuário se token válido, None caso contrário
    """
    if not credentials:
        return None
    
    token = credentials.credentials
    return decode_jwt_token(token)


def require_role(required_roles: list[str]):
    """
    Decorator factory para exigir roles específicas
    Uso: @require_role(["Admin", "Manager"])
    
    Args:
        required_roles: Lista de roles aceitas
        
    Returns:
        Função decoradora
    """
    async def role_checker(current_user: Dict[str, Any] = Depends(get_current_user)):
        # Claims de role podem vir como 'role' ou 'roles' (pode ser string ou array)
        user_roles = current_user.get("role") or current_user.get("roles") or []
        
        # Normaliza para lista
        if isinstance(user_roles, str):
            user_roles = [user_roles]
        
        # Verifica se tem alguma das roles requeridas
        if not any(role in user_roles for role in required_roles):
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail=f"Acesso negado. Requer uma das roles: {', '.join(required_roles)}"
            )
        
        return current_user
    
    return role_checker


# === UTILITÁRIOS ===

def extract_user_id(payload: Dict[str, Any]) -> Optional[str]:
    """
    Extrai o ID do usuário do payload JWT
    O C# pode usar diferentes claims: 'sub', 'nameid', 'id', etc.
    
    Args:
        payload: Payload decodificado do JWT
        
    Returns:
        ID do usuário ou None
    """
    return (
        payload.get("sub") or 
        payload.get("nameid") or 
        payload.get("id") or 
        payload.get("userId")
    )


def extract_user_email(payload: Dict[str, Any]) -> Optional[str]:
    """
    Extrai o email do usuário do payload JWT
    
    Args:
        payload: Payload decodificado do JWT
        
    Returns:
        Email do usuário ou None
    """
    return payload.get("email") or payload.get("unique_name")
