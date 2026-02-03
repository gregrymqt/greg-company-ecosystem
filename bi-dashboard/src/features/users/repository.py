from typing import List, Dict, Any
from sqlalchemy import text
from ...core.infrastructure.database import get_db_session

class UsersRepository:
    """
    Repository Assíncrono para Usuários (Greg Company 2.0)
    Stack: SQLAlchemy Async + aioodbc
    """
    
    async def get_users_summary_stats(self) -> Dict[str, Any]:
        """
        Calcula KPIs diretamente no Banco (MUITO mais rápido que contar lista).
        """
        query = text("""
        SELECT 
            COUNT(*) as TotalUsers,
            SUM(CASE WHEN EmailConfirmed = 1 THEN 1 ELSE 0 END) as ConfirmedEmails,
            SUM(CASE WHEN CreatedAt >= DATEADD(day, -30, GETDATE()) THEN 1 ELSE 0 END) as NewUsersLast30Days
        FROM AspNetUsers
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            row = result.fetchone()
            return dict(row._mapping) if row else {}

    async def get_latest_users(self, limit: int = 100) -> List[Dict[str, Any]]:
        """Busca apenas os últimos usuários para listagem"""
        query = text(f"""
        SELECT TOP {limit}
            Id,
            Name,
            Email,
            EmailConfirmed,
            CreatedAt
        FROM AspNetUsers
        ORDER BY CreatedAt DESC
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            return [dict(row._mapping) for row in result]