import os
from sqlalchemy.ext.asyncio import create_async_engine, async_sessionmaker, AsyncSession
from sqlalchemy.orm import declarative_base
from sqlalchemy.pool import NullPool

# Lê a connection string definida na infraestrutura (porta 6543)
DATABASE_URL = os.getenv("POSTGRES_URI")

# Se vier com o prefixo 'postgresql://', substitui para o driver assíncrono do 'asyncpg'
if DATABASE_URL and DATABASE_URL.startswith("postgresql://"):
    DATABASE_URL = DATABASE_URL.replace("postgresql://", "postgresql+asyncpg://", 1)

# Usamos NullPool para evitar conexões persistentes ociosas nos workers e API
engine = create_async_engine(
    DATABASE_URL,
    echo=False,
    poolclass=NullPool
)

AsyncSessionLocal = async_sessionmaker(
    bind=engine,
    class_=AsyncSession,
    expire_on_commit=False
)

Base = declarative_base()

# Dependência para endpoints FastAPI (se houver)
async def get_db():
    async with AsyncSessionLocal() as session:
        yield session
