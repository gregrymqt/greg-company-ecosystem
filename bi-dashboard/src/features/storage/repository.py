from typing import List, Dict, Any, Optional
from sqlalchemy import text
from ...core.infrastructure.database import get_db_session # Helper Async

class StorageRepository:
    """
    Repository Assíncrono para Storage (Greg Company 2.0)
    Stack: SQLAlchemy Async + aioodbc
    """
    
    async def get_total_storage_stats(self) -> Dict[str, Any]:
        """Busca estatísticas totais (Non-blocking)"""
        query = text("""
        SELECT 
            COUNT(*) AS TotalFiles,
            ISNULL(SUM(TamanhoBytes), 0) AS TotalBytes,
            ISNULL(SUM(TamanhoBytes) / 1024.0 / 1024.0 / 1024.0, 0) AS TotalGB,
            ISNULL(SUM(TamanhoBytes) / 1024.0 / 1024.0, 0) AS TotalMB
        FROM EntityFiles
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            row = result.fetchone()
            # Mapeamento manual para garantir tipos
            if row:
                return {
                    'TotalFiles': row[0] or 0,
                    'TotalBytes': int(row[1]) if row[1] else 0,
                    'TotalGB': float(row[2]) if row[2] else 0.0,
                    'TotalMB': float(row[3]) if row[3] else 0.0
                }
            return {'TotalFiles': 0, 'TotalBytes': 0, 'TotalGB': 0.0, 'TotalMB': 0.0}

    async def get_storage_by_category(self) -> List[Dict[str, Any]]:
        """Agrupamento por categoria"""
        query = text("""
        SELECT 
            ISNULL(FeatureCategoria, 'Sem Categoria') AS FeatureCategoria,
            COUNT(*) AS TotalFiles,
            ISNULL(SUM(TamanhoBytes), 0) AS TotalBytes,
            ISNULL(SUM(TamanhoBytes) / 1024.0 / 1024.0 / 1024.0, 0) AS TotalGB,
            ISNULL(AVG(TamanhoBytes) / 1024.0 / 1024.0, 0) AS AvgFileSizeMB
        FROM EntityFiles
        GROUP BY FeatureCategoria
        ORDER BY TotalBytes DESC
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            return [dict(row._mapping) for row in result]

    async def get_largest_files(self, limit: int = 10) -> List[Dict[str, Any]]:
        """Top X arquivos pesados"""
        query = text(f"""
        SELECT TOP {limit}
            Id, FileName, FeatureCategoria, TamanhoBytes,
            TamanhoBytes / 1024.0 / 1024.0 AS SizeMB,
            CriadoEm, ModificadoEm
        FROM EntityFiles
        ORDER BY TamanhoBytes DESC
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            return [dict(row._mapping) for row in result]

    async def get_storage_growth_trend(self, days: int = 30) -> List[Dict[str, Any]]:
        """Tendência de crescimento"""
        query = text(f"""
        SELECT 
            CAST(CriadoEm AS DATE) AS Date,
            COUNT(*) AS FilesAdded,
            ISNULL(SUM(TamanhoBytes) / 1024.0 / 1024.0 / 1024.0, 0) AS GBAdded
        FROM EntityFiles
        WHERE CriadoEm >= DATEADD(DAY, -{days}, GETDATE())
        GROUP BY CAST(CriadoEm AS DATE)
        ORDER BY Date ASC
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            return [dict(row._mapping) for row in result]