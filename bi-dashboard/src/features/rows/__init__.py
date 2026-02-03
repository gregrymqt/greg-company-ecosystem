"""
Inicialização do pacote 'rows'.

Este arquivo `__init__.py` serve para duas coisas:

1.  **Define o diretório como um pacote Python**, permitindo que seus módulos
    sejam importados de outros lugares (ex: `from features.rows import ...`).

2.  **Simplifica as importações**. Ao expor as classes principais aqui,
    outras partes do código podem importar diretamente deste pacote, sem
    precisar conhecer a estrutura interna dos arquivos.

    Exemplo:
    - Em vez de: `from features.rows.rows_schemas import RowsUpdateRequest`
    - Usamos:   `from features.rows import RowsUpdateRequest`
"""
from .schemas import RowsUpdateRequest, RowsCellValue
from .service import rows_service

__all__ = [
    'RowsUpdateRequest',
    'RowsCellValue',
    'rows_service',
]