"""
Storage Feature
Analytics de armazenamento baseado em EntityFile.cs
"""

from .service import StorageService, create_storage_service
from .repository import StorageRepository
from .schemas import StorageStatsDTO, FileCategoryBreakdownDTO, FileDetailDTO

__all__ = [
    'StorageService',
    'create_storage_service',
    'StorageRepository',
    'StorageStatsDTO',
    'FileCategoryBreakdownDTO',
    'FileDetailDTO',
]
