"""
Storage Repository
Repositório para acesso aos dados de armazenamento (EntityFile)
Baseado em EntityFile.cs do backend
"""

from typing import List, Dict, Optional, Any
from sqlalchemy import text
from ...core.infrastructure import get_db_session, get_db_engine
import pandas as pd


class StorageRepository:
    """
    Repository para operações de storage/armazenamento
    Foca em queries de leitura (BI/Analytics)
    """
    
    def __init__(self):
        self.engine = get_db_engine()
    
    # ==================== STORAGE STATS ====================
    
    def get_total_storage_stats(self) -> Dict[str, Any]:
        """
        Busca estatísticas totais de armazenamento
        Campos baseados em EntityFile.cs
        """
        query = """
        SELECT 
            COUNT(*) AS TotalFiles,
            ISNULL(SUM(TamanhoBytes), 0) AS TotalBytes,
            ISNULL(SUM(TamanhoBytes) / 1024.0 / 1024.0 / 1024.0, 0) AS TotalGB,
            ISNULL(SUM(TamanhoBytes) / 1024.0 / 1024.0, 0) AS TotalMB
        FROM EntityFiles
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            row = result.fetchone()
            
            if row:
                return {
                    'TotalFiles': row[0] or 0,
                    'TotalBytes': int(row[1]) if row[1] else 0,
                    'TotalGB': float(row[2]) if row[2] else 0.0,
                    'TotalMB': float(row[3]) if row[3] else 0.0
                }
            
            return {
                'TotalFiles': 0,
                'TotalBytes': 0,
                'TotalGB': 0.0,
                'TotalMB': 0.0
            }
    
    def get_storage_by_category(self) -> List[Dict[str, Any]]:
        """
        Busca espaço utilizado agrupado por FeatureCategoria
        Ex: Vídeos vs. Imagens vs. Documentos
        """
        query = """
        SELECT 
            ISNULL(FeatureCategoria, 'Sem Categoria') AS FeatureCategoria,
            COUNT(*) AS TotalFiles,
            ISNULL(SUM(TamanhoBytes), 0) AS TotalBytes,
            ISNULL(SUM(TamanhoBytes) / 1024.0 / 1024.0 / 1024.0, 0) AS TotalGB,
            ISNULL(AVG(TamanhoBytes) / 1024.0 / 1024.0, 0) AS AvgFileSizeMB
        FROM EntityFiles
        GROUP BY FeatureCategoria
        ORDER BY TotalBytes DESC
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            rows = result.fetchall()
            
            return [
                {
                    'FeatureCategoria': row[0],
                    'TotalFiles': row[1],
                    'TotalBytes': int(row[2]) if row[2] else 0,
                    'TotalGB': float(row[3]) if row[3] else 0.0,
                    'AvgFileSizeMB': float(row[4]) if row[4] else 0.0
                }
                for row in rows
            ]
    
    def get_largest_files(self, limit: int = 10) -> List[Dict[str, Any]]:
        """
        Busca os maiores arquivos no sistema
        Útil para identificar consumo excessivo
        """
        query = f"""
        SELECT TOP {limit}
            Id,
            FileName,
            FeatureCategoria,
            TamanhoBytes,
            TamanhoBytes / 1024.0 / 1024.0 AS SizeMB,
            CriadoEm,
            ModificadoEm
        FROM EntityFiles
        ORDER BY TamanhoBytes DESC
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            rows = result.fetchall()
            
            return [
                {
                    'Id': row[0],
                    'FileName': row[1],
                    'FeatureCategoria': row[2],
                    'TamanhoBytes': int(row[3]) if row[3] else 0,
                    'SizeMB': float(row[4]) if row[4] else 0.0,
                    'CriadoEm': row[5].isoformat() if row[5] else None,
                    'ModificadoEm': row[6].isoformat() if row[6] else None
                }
                for row in rows
            ]
    
    def get_files_by_category(self, categoria: str, limit: Optional[int] = 50) -> List[Dict[str, Any]]:
        """
        Busca arquivos filtrados por categoria específica
        """
        limit_clause = f"TOP {limit}" if limit else ""
        
        query = f"""
        SELECT {limit_clause}
            Id,
            FileName,
            FeatureCategoria,
            TamanhoBytes,
            TamanhoBytes / 1024.0 / 1024.0 AS SizeMB,
            CriadoEm,
            ModificadoEm
        FROM EntityFiles
        WHERE FeatureCategoria = :categoria
        ORDER BY TamanhoBytes DESC
        """
        
        with get_db_session() as session:
            result = session.execute(text(query), {'categoria': categoria})
            rows = result.fetchall()
            
            return [
                {
                    'Id': row[0],
                    'FileName': row[1],
                    'FeatureCategoria': row[2],
                    'TamanhoBytes': int(row[3]) if row[3] else 0,
                    'SizeMB': float(row[4]) if row[4] else 0.0,
                    'CriadoEm': row[5].isoformat() if row[5] else None,
                    'ModificadoEm': row[6].isoformat() if row[6] else None
                }
                for row in rows
            ]
    
    # ==================== STORAGE TRENDS ====================
    
    def get_storage_growth_trend(self, days: int = 30) -> List[Dict[str, Any]]:
        """
        Analisa crescimento de armazenamento nos últimos X dias
        Útil para prever necessidade de expansão
        """
        query = f"""
        SELECT 
            CAST(CriadoEm AS DATE) AS Date,
            COUNT(*) AS FilesAdded,
            ISNULL(SUM(TamanhoBytes) / 1024.0 / 1024.0 / 1024.0, 0) AS GBAdded
        FROM EntityFiles
        WHERE CriadoEm >= DATEADD(DAY, -{days}, GETDATE())
        GROUP BY CAST(CriadoEm AS DATE)
        ORDER BY Date ASC
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            rows = result.fetchall()
            
            return [
                {
                    'Date': row[0].isoformat() if row[0] else None,
                    'FilesAdded': row[1],
                    'GBAdded': float(row[2]) if row[2] else 0.0
                }
                for row in rows
            ]
