from pydantic import BaseModel, Field, HttpUrl
from typing import Optional, List, Dict
from datetime import datetime
from enum import Enum

class ProductStatus(str, Enum):
    RAW = "raw"
    PROCESSED = "processed"
    PUBLISHED = "published"
    ERROR = "error"

class Product(BaseModel):
    # Identificadores e Rastreabilidade
    id: Optional[str] = Field(None, alias="_id")
    sku: str = Field(..., min_length=3)
    source_url: HttpUrl  # Valida se é uma URL real
    
    # Informações de Negócio
    name: str
    description: str
    cost_price: float = Field(..., gt=0) # Preço deve ser maior que zero
    currency: str = "BRL"
    
    # Dados de Mídia e Categorização
    images: List[HttpUrl] = []
    category: str = "Geral"
    
    # Controle de Pipeline
    status: ProductStatus = ProductStatus.RAW
    created_at: datetime = Field(default_factory=datetime.utcnow)
    last_error: Optional[str] = None

    class Config:
        populate_by_name = True # Permite usar o nome do campo ou alias