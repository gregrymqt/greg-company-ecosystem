"""
Storage Analytics DTOs (Schemas)
Data Transfer Objects para análises de armazenamento
Baseado em EntityFile.cs
"""

from dataclasses import dataclass
from typing import List, Optional
from decimal import Decimal


@dataclass
class FileCategoryBreakdownDTO:
    """
    DTO para breakdown de espaço utilizado por categoria
    """
    FeatureCategoria: str
    TotalFiles: int
    TotalBytes: int
    TotalGB: Decimal
    PercentageOfTotal: Decimal
    AvgFileSizeMB: Decimal


@dataclass
class StorageStatsDTO:
    """
    DTO para estatísticas gerais de armazenamento
    """
    TotalFiles: int
    TotalBytes: int
    TotalGB: Decimal
    TotalMB: Decimal
    CategoryBreakdown: List[FileCategoryBreakdownDTO]
    LargestCategory: Optional[str] = None
    SmallestCategory: Optional[str] = None


@dataclass
class FileDetailDTO:
    """
    DTO para detalhes individuais de arquivo
    """
    Id: int
    FileName: str
    FeatureCategoria: str
    TamanhoBytes: int
    SizeMB: Decimal
    CriadoEm: str
    ModificadoEm: Optional[str] = None
