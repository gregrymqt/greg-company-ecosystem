from typing import List, Optional
from pydantic import BaseModel, Field

class ShopifyOptionValueInput(BaseModel):
    optionName: str = Field(..., description="Nome da opção, ex: 'Color'")
    name: str = Field(..., description="Valor da opção, ex: 'Blue'")

class ShopifyVariantInput(BaseModel):
    price: str
    sku: Optional[str] = None
    inventoryItem: Optional[dict] = Field(None, description="Dados de estoque e rastreamento")
    optionValues: List[ShopifyOptionValueInput]

class ShopifyProductOptionInput(BaseModel):
    name: str
    values: List[dict] # Ex: [{"name": "Blue"}, {"name": "Red"}]

class ShopifyProductSetInput(BaseModel):
    """
    Input principal para a mutação productSet (Stable 2024-04).
    Ideal para sincronização externa (ERP/Bot).
    """
    title: str
    descriptionHtml: str
    vendor: str
    productType: Optional[str] = None
    status: str = "DRAFT" # ACTIVE, ARCHIVED, DRAFT
    productOptions: List[ShopifyProductOptionInput] = []
    variants: List[ShopifyVariantInput] = []
    seoTitle: Optional[str] = None
    seoDescription: Optional[str] = None
    tags: Optional[str] = None

    @classmethod
    def from_internal_data(cls, data: dict):
        tags = data.get("tags", "")
        tags_str = ",".join(tags) if isinstance(tags, list) else str(tags)
        
        return cls(
            title=data.get("title", ""),
            descriptionHtml=data.get("description", ""),
            vendor=data.get("vendor", "Default Vendor"),
            status="DRAFT",
            productOptions=[],
            variants=[
                ShopifyVariantInput(
                    price=str(data.get("price", 0.0)),
                    sku=data.get("sku", ""),
                    optionValues=[]
                )
            ],
            seoTitle=data.get("seo_title", data.get("title", "")),
            seoDescription=data.get("seo_description", ""),
            tags=tags_str
        )

class ShopifyGraphQLVariables(BaseModel):
    input: ShopifyProductSetInput

class ShopifyGraphQLRequest(BaseModel):
    """
    Payload final enviado via POST para /admin/api/2024-04/graphql.json
    """
    query: str = """
    mutation productSet($input: ProductSetInput!) {
      productSet(input: $input) {
        product {
          id
          title
        }
        userErrors {
          field
          message
        }
      }
    }
    """
    variables: ShopifyGraphQLVariables

