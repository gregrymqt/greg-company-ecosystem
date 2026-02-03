from pydantic import BaseModel, Field
from typing import List, Union, Optional

# 1. Representa o valor de uma única célula
class RowsCellValue(BaseModel):
    value: Union[str, int, float, bool, None] = None
    formula: Optional[str] = None # Opcional, caso queira enviar fórmula

# 2. Representa a estrutura de envio (Body do Request)
# O Rows exige: { "cells": [ [ {value: ...}, {value: ...} ], ... ] }
class RowsUpdateRequest(BaseModel):
    cells: List[List[RowsCellValue]]

    class Config:
        # Exemplo para aparecer no Swagger se precisasse
        json_schema_extra = {
            "example": {
                "cells": [
                    [{"value": "Receita"}, {"value": 1500.50}],
                    [{"value": "Despesa"}, {"value": 800.00}]
                ]
            }
        }