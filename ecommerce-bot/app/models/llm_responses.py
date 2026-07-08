from pydantic import BaseModel, Field

class EnrichedProductResponse(BaseModel):
    description: str = Field(description="A nova descrição persuasiva e traduzida do produto.")
