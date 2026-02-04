import asyncio
from typing import List, Dict, Any

# Infra Nova
from ...core.infrastructure.rows_client import rows_client
from ...features.rows.service import rows_service
from ...core.infrastructure.redis_client import get_or_create_async

# Feature Imports
from .repository import ContentRepository
from .schemas import CourseDTO, ContentSummaryDTO

class ContentService:
    def __init__(self, repository: ContentRepository):
        self.repository = repository

    # ==================== MÉTODOS DE NEGÓCIO (ASYNC) ====================

    async def get_courses_list(self, limit: int = 100) -> List[CourseDTO]:
        """Busca lista de cursos (bypass de cache para listagens detalhadas)"""
        raw_courses = await self.repository.get_all_courses(limit)
        
        return [
            CourseDTO(
                Id=str(row['Id']),
                Name=row['Name'],
                IsActive=bool(row['IsActive']),
                TotalVideos=row['TotalVideos']
            ) for row in raw_courses
        ]

    async def get_content_kpis(self, use_cache: bool = True) -> ContentSummaryDTO:
        cache_key = "content:kpis:summary"
        
        async def _calculate_kpis():
            # ✅ BUSCA DIRETA: Nada de loops ou limites artificiais
            raw = await self.repository.get_dashboard_metrics()
            
            total_courses = raw.get('TotalCourses', 0)
            active_courses = raw.get('ActiveCourses', 0)
            total_videos = raw.get('TotalVideosLib', 0)
            
            avg_videos = 0.0
            if total_courses > 0:
                avg_videos = total_videos / total_courses

            return ContentSummaryDTO(
                TotalCourses=total_courses,
                ActiveCourses=active_courses,
                TotalVideosLib=total_videos,
                AvgVideosPerCourse=round(avg_videos, 1) # Arredondamento visual
            )

        if use_cache:
            return await get_or_create_async(cache_key, _calculate_kpis, ttl_seconds=300)
        
        return await _calculate_kpis()

    # ==================== SYNC ROWS (PARALELO) ====================

    async def sync_content_to_rows(self):
        """
        Sincroniza KPIs e Lista de Cursos com o Rows em paralelo.
        """
        # Executa as duas buscas ao mesmo tempo (Scatter-Gather)
        summary_task = self.get_content_kpis(use_cache=False) # Fresco para o relatório
        courses_task = self.get_courses_list(limit=50)
        
        summary, courses_list = await asyncio.gather(summary_task, courses_task)

        # Prepara payloads
        payload_kpis = rows_service.build_content_kpis(summary)
        payload_list = rows_service.build_courses_list_payload(courses_list)

        # Envia para o Rows em paralelo
        await asyncio.gather(
            rows_client.send_data("Content_KPIs!A1", payload_kpis),
            rows_client.send_data("Content_Courses!A1", payload_list)
        )

        return {
            "status": "success",
            "message": f"Content synced: {len(courses_list)} courses listed."
        }

def create_content_service() -> ContentService:
    return ContentService(ContentRepository())