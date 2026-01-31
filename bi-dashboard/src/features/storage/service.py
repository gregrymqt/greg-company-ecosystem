"""
Storage Service
Lógica de negócio para análises de armazenamento
"""

from typing import List, Optional
from decimal import Decimal
from .repository import StorageRepository
from .schemas import StorageStatsDTO, FileCategoryBreakdownDTO, FileDetailDTO


class StorageService:
    """
    Service para processamento de dados de armazenamento
    """
    
    def __init__(self, repository: StorageRepository):
        self.repository = repository
    
    def get_storage_overview(self) -> StorageStatsDTO:
        """
        Retorna visão geral de armazenamento com breakdown por categoria
        """
        # Busca stats totais
        total_stats = self.repository.get_total_storage_stats()
        
        # Busca breakdown por categoria
        category_data = self.repository.get_storage_by_category()
        
        # Calcula percentual de cada categoria
        total_bytes = total_stats['TotalBytes']
        
        category_breakdown = []
        for cat in category_data:
            percentage = (cat['TotalBytes'] / total_bytes * 100) if total_bytes > 0 else 0
            
            category_breakdown.append(FileCategoryBreakdownDTO(
                FeatureCategoria=cat['FeatureCategoria'],
                TotalFiles=cat['TotalFiles'],
                TotalBytes=cat['TotalBytes'],
                TotalGB=Decimal(str(cat['TotalGB'])).quantize(Decimal('0.01')),
                PercentageOfTotal=Decimal(str(percentage)).quantize(Decimal('0.01')),
                AvgFileSizeMB=Decimal(str(cat['AvgFileSizeMB'])).quantize(Decimal('0.01'))
            ))
        
        # Identifica maior e menor categoria
        largest_cat = None
        smallest_cat = None
        
        if category_breakdown:
            largest_cat = max(category_breakdown, key=lambda x: x.TotalBytes).FeatureCategoria
            smallest_cat = min(category_breakdown, key=lambda x: x.TotalBytes).FeatureCategoria
        
        return StorageStatsDTO(
            TotalFiles=total_stats['TotalFiles'],
            TotalBytes=total_stats['TotalBytes'],
            TotalGB=Decimal(str(total_stats['TotalGB'])).quantize(Decimal('0.01')),
            TotalMB=Decimal(str(total_stats['TotalMB'])).quantize(Decimal('0.01')),
            CategoryBreakdown=category_breakdown,
            LargestCategory=largest_cat,
            SmallestCategory=smallest_cat
        )
    
    def get_largest_files(self, limit: int = 10) -> List[FileDetailDTO]:
        """
        Retorna os maiores arquivos do sistema
        """
        files = self.repository.get_largest_files(limit)
        
        return [
            FileDetailDTO(
                Id=file['Id'],
                FileName=file['FileName'],
                FeatureCategoria=file['FeatureCategoria'],
                TamanhoBytes=file['TamanhoBytes'],
                SizeMB=Decimal(str(file['SizeMB'])).quantize(Decimal('0.01')),
                CriadoEm=file['CriadoEm'],
                ModificadoEm=file['ModificadoEm']
            )
            for file in files
        ]
    
    def get_files_by_category(self, categoria: str, limit: Optional[int] = 50) -> List[FileDetailDTO]:
        """
        Retorna arquivos de uma categoria específica
        """
        files = self.repository.get_files_by_category(categoria, limit)
        
        return [
            FileDetailDTO(
                Id=file['Id'],
                FileName=file['FileName'],
                FeatureCategoria=file['FeatureCategoria'],
                TamanhoBytes=file['TamanhoBytes'],
                SizeMB=Decimal(str(file['SizeMB'])).quantize(Decimal('0.01')),
                CriadoEm=file['CriadoEm'],
                ModificadoEm=file['ModificadoEm']
            )
            for file in files
        ]
    
    def get_storage_growth_trend(self, days: int = 30) -> dict:
        """
        Retorna tendência de crescimento de armazenamento
        """
        trend_data = self.repository.get_storage_growth_trend(days)
        
        total_gb_added = sum(item['GBAdded'] for item in trend_data)
        total_files_added = sum(item['FilesAdded'] for item in trend_data)
        
        return {
            'TrendData': trend_data,
            'Summary': {
                'Days': days,
                'TotalGBAdded': round(total_gb_added, 2),
                'TotalFilesAdded': total_files_added,
                'AvgGBPerDay': round(total_gb_added / days, 2) if days > 0 else 0
            }
        }


# Factory pattern (seguindo padrão das outras features)
def create_storage_service() -> StorageService:
    """
    Factory para criar instância do StorageService
    """
    return StorageService(StorageRepository())
