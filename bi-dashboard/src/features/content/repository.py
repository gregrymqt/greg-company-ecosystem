"""
Content Repository
Queries para cursos e vídeos
"""

from typing import List, Dict, Any
from sqlalchemy import text
from ...core.infrastructure import get_db_session


class ContentRepository:
    """Repository para conteúdo"""
    
    def get_all_courses(self, limit: int = 100) -> List[Dict[str, Any]]:
        """Busca cursos"""
        query = f"""
        SELECT 
            c.Id,
            c.Name,
            c.IsActive,
            COUNT(v.Id) AS TotalVideos
        FROM Courses c
        LEFT JOIN Videos v ON c.Id = v.CourseId
        GROUP BY c.Id, c.Name, c.IsActive
        ORDER BY c.Name
        OFFSET 0 ROWS FETCH NEXT {limit} ROWS ONLY
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            return [dict(row._mapping) for row in result]
