from typing import List, Dict, Any
from sqlalchemy import text
from ...core.infrastructure.database import get_db_session # O helper async que criamos

class ContentRepository:
    """
    Repository Assíncrono para Conteúdo
    Stack: SQLAlchemy Async + aioodbc
    """
    
    async def get_all_courses(self, limit: int = 100) -> List[Dict[str, Any]]:
        """
        Busca cursos e contagem de vídeos de forma não bloqueante.
        """
        query = text(f"""
        SELECT 
            c.Id,
            c.Name,
            c.IsActive,
            COUNT(v.Id) AS TotalVideos
        FROM Courses c
        LEFT JOIN Videos v ON c.Id = v.CourseId
        GROUP BY c.Id, c.Name, c.IsActive
        ORDER BY c.Name
        OFFSET 0 ROWS FETCH NEXT :limit ROWS ONLY
        """)
        
        # Async Context Manager
        async with get_db_session() as session:
            # O banco processa, o Python fica livre
            result = await session.execute(query, {"limit": limit})
            return [dict(row._mapping) for row in result]
        

    async def get_dashboard_metrics(self) -> Dict[str, Any]:
        """
        Retorna os KPIs de conteúdo em uma única ida ao banco.
        Usa Subqueries para eficiência total.
        """
        query = text("""
        SELECT 
            (SELECT COUNT(*) FROM Courses) AS TotalCourses,
            (SELECT COUNT(*) FROM Courses WHERE IsActive = 1) AS ActiveCourses,
            (SELECT COUNT(*) FROM Videos) AS TotalVideosLib
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            row = await result.fetchone()
            return dict(row._mapping) if row else {}    