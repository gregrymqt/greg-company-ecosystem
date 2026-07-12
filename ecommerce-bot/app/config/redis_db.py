import json
import asyncio
from typing import Any, Optional, Callable
import redis.asyncio as redis
from app.config.settings import settings
import logging

logger = logging.getLogger(__name__)

class RedisCache:
    def __init__(self):
        self.redis_client: Optional[redis.Redis] = None

    async def connect(self):
        logger.info(f"Connecting to Redis at {settings.REDIS_URL}")
        self.redis_client = redis.from_url(settings.REDIS_URL, decode_responses=True)
        await self.redis_client.ping()
        logger.info("Connected to Redis successfully.")

    async def disconnect(self):
        if self.redis_client:
            logger.info("Disconnecting from Redis...")
            await self.redis_client.close()

    async def get(self, key: str) -> Optional[Any]:
        if not self.redis_client:
            return None
        value = await self.redis_client.get(key)
        if value:
            try:
                return json.loads(value)
            except json.JSONDecodeError:
                return value
        return None

    async def set(self, key: str, value: Any, expire_seconds: int = 3600):
        if not self.redis_client:
            return
        
        if isinstance(value, (dict, list)):
            value = json.dumps(value)
            
        await self.redis_client.set(key, value, ex=expire_seconds)

    async def get_or_create(self, key: str, factory: Callable, expire_seconds: int = 3600) -> Any:
        value = await self.get(key)
        if value is not None:
            return value

        # Create new value
        if asyncio.iscoroutinefunction(factory):
            new_value = await factory()
        else:
            new_value = factory()
            
        if new_value is not None:
            await self.set(key, new_value, expire_seconds)
            
        return new_value

# Instância global
redis_cache = RedisCache()
