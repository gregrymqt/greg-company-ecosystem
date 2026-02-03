import asyncio
from typing import List, Optional
from decimal import Decimal

# Infra Greg Company
from ...core.infrastructure.rows_client import rows_client
from ...features.rows.service import rows_service
from ...core.infrastructure.redis_client import get_or_create_async

from .repository import StorageRepository
from .schemas import StorageStatsDTO, FileCategoryBreakdownDTO, FileDetailDTO

class StorageService:
    def __init__(self, repository: StorageRepository):
        self.repository = repository
    
    # ==================== OVERVIEW COM CACHE ====================

    async def get_storage_overview(self, use_cache: bool = True) -> StorageStatsDTO:
        """
        Retorna visão geral. Cache de 5 minutos por padrão.
        """
        cache_key = "storage:overview:stats"

        # Função Factory: Roda apenas no Cache MISS
        async def _calculate_overview():
            # Executa as duas queries em paralelo (Performance Boost)
            task_totals = self.repository.get_total_storage_stats()
            task_categories = self.repository.get_storage_by_category()
            
            total_stats, category_data = await asyncio.gather(task_totals, task_categories)
            
            # Lógica de Negócio (Conversão para DTO)
            total_bytes = total_stats['TotalBytes']
            category_breakdown = []
            
            for cat in category_data:
                p = (cat['TotalBytes'] / total_bytes * 100) if total_bytes > 0 else 0
                
                category_breakdown.append(FileCategoryBreakdownDTO(
                    FeatureCategoria=cat['FeatureCategoria'],
                    TotalFiles=cat['TotalFiles'],
                    TotalBytes=cat['TotalBytes'],
                    TotalGB=Decimal(str(cat['TotalGB'])).quantize(Decimal('0.01')),
                    PercentageOfTotal=Decimal(str(p)).quantize(Decimal('0.01')),
                    AvgFileSizeMB=Decimal(str(cat['AvgFileSizeMB'])).quantize(Decimal('0.01'))
                ))
            
            largest_cat = max(category_breakdown, key=lambda x: x.TotalBytes).FeatureCategoria if category_breakdown else None
            smallest_cat = min(category_breakdown, key=lambda x: x.TotalBytes).FeatureCategoria if category_breakdown else None
            
            return StorageStatsDTO(
                TotalFiles=total_stats['TotalFiles'],
                TotalBytes=total_stats['TotalBytes'],
                TotalGB=Decimal(str(total_stats['TotalGB'])).quantize(Decimal('0.01')),
                TotalMB=Decimal(str(total_stats['TotalMB'])).quantize(Decimal('0.01')),
                CategoryBreakdown=category_breakdown,
                LargestCategory=largest_cat,
                SmallestCategory=smallest_cat
            )

        if use_cache:
            return await get_or_create_async(cache_key, _calculate_overview, ttl_seconds=300)
        return await _calculate_overview()

    # ==================== LISTAGENS (SEM CACHE OU CURTO) ====================

    async def get_largest_files(self, limit: int = 10) -> List[FileDetailDTO]:
        """Geralmente real-time, mas pode cachear se quiser"""
        files = await self.repository.get_largest_files(limit)
        return [self._map_file_to_dto(f) for f in files]

    # Helper privado para mapeamento
    def _map_file_to_dto(self, file_dict: dict) -> FileDetailDTO:
        return FileDetailDTO(
            Id=file_dict['Id'],
            FileName=file_dict['FileName'],
            FeatureCategoria=file_dict['FeatureCategoria'],
            TamanhoBytes=file_dict['TamanhoBytes'],
            SizeMB=Decimal(str(file_dict['SizeMB'])).quantize(Decimal('0.01')),
            CriadoEm=file_dict['CriadoEm'],
            ModificadoEm=file_dict['ModificadoEm']
        )

    # ==================== ROWS SYNC (PARALLEL) ====================

    async def sync_storage_to_rows(self):
        # 1. Busca dados (Force update para garantir frescor no relatório)
        # Executa em paralelo: Overview e LargestFiles
        task_overview = self.get_storage_overview(use_cache=False)
        task_files = self.get_largest_files(limit=20)
        
        overview_dto, largest_files_dto = await asyncio.gather(task_overview, task_files)

        # 2. Formata Payloads
        payload_kpis = rows_service.build_storage_kpis(overview_dto)
        payload_files = rows_service.build_largest_files_list(largest_files_dto)

        # 3. Envia para o Rows
        await asyncio.gather(
            rows_client.send_data("Storage_KPIs!A1", payload_kpis),
            rows_client.send_data("Storage_TopFiles!A1", payload_files)
        )

        return {"status": "success", "message": "Storage synced."}

def create_storage_service() -> StorageService:
    return StorageService(StorageRepository())