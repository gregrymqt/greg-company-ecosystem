"""
Users Repository
Queries para usuários
"""

from typing import List, Dict, Any
from sqlalchemy import text
from ...core.infrastructure import get_db_session


class UsersRepository:
    """Repository para usuários"""
    
    def get_all_users(self, limit: int = 100) -> List[Dict[str, Any]]:
        """Busca usuários"""
        query = f"""
        SELECT 
            Id,
            Name,
            Email,
            EmailConfirmed,
            CreatedAt
        FROM AspNetUsers
        ORDER BY CreatedAt DESC
        OFFSET 0 ROWS FETCH NEXT {limit} ROWS ONLY
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            return [dict(row._mapping) for row in result]
