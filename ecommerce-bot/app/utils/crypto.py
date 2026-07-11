import os
import base64
import logging
from cryptography.hazmat.primitives.ciphers.aead import AESGCM

logger = logging.getLogger(__name__)

def _get_key() -> bytes:
    key_b64 = os.environ.get("AES_MASTER_KEY")
    if not key_b64:
        raise ValueError("AES_MASTER_KEY não configurada no ambiente.")
    return base64.b64decode(key_b64)

def encrypt_api_key(plain_text: str) -> str:
    """Criptografa uma string usando AES-256-GCM."""
    if not plain_text:
        return plain_text
    key = _get_key()
    aesgcm = AESGCM(key)
    nonce = os.urandom(12)
    cipher_text = aesgcm.encrypt(nonce, plain_text.encode('utf-8'), None)
    return base64.b64encode(nonce + cipher_text).decode('utf-8')

def decrypt_api_key(cipher_text_b64: str) -> str:
    """Descriptografa uma string usando AES-256-GCM."""
    if not cipher_text_b64:
        return cipher_text_b64
    try:
        key = _get_key()
        aesgcm = AESGCM(key)
        data = base64.b64decode(cipher_text_b64)
        nonce = data[:12]
        cipher_text = data[12:]
        plain_text = aesgcm.decrypt(nonce, cipher_text, None)
        return plain_text.decode('utf-8')
    except Exception as e:
        logger.error(f"Erro ao descriptografar chave: {e}")
        # Retorna a chave original caso a mesma não esteja criptografada (fallback backward-compatibility)
        return cipher_text_b64
