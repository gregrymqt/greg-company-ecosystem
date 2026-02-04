from typing import List, Dict, Any
from sqlalchemy import text
from ...core.infrastructure.database import get_db_session # Helper Async

class SubscriptionsRepository:
    """
    Repository Assíncrono para Assinaturas
    Stack: SQLAlchemy Async + aioodbc
    """
    
    async def get_subscriptions_summary(self) -> Dict[str, Any]:
        """Resumo de assinaturas e MRR (Non-blocking)"""
        query = text("""
        SELECT 
            COUNT(*) AS TotalSubscriptions,
            SUM(CASE WHEN Status = 'authorized' THEN 1 ELSE 0 END) AS ActiveSubscriptions,
            SUM(CASE WHEN Status = 'cancelled' THEN 1 ELSE 0 END) AS CancelledSubscriptions,
            SUM(CASE WHEN Status = 'paused' THEN 1 ELSE 0 END) AS PausedSubscriptions,
            ISNULL(SUM(CASE WHEN Status = 'authorized' THEN CurrentAmount ELSE 0 END), 0) AS MonthlyRecurringRevenue
        FROM Subscriptions
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            row = await result.fetchone()
            return dict(row._mapping) if row else {}

    async def get_recent_subscriptions(self, limit: int = 50) -> List[Dict[str, Any]]:
        """Busca as assinaturas mais recentes para a lista detalhada"""
        query = text(f"""
        SELECT TOP {limit}
            s.Id, s.UserId, s.Status, s.CurrentAmount, 
            s.CurrentPeriodEndDate, s.PlanName, 
            u.Name as UserName, u.Email as UserEmail
        FROM Subscriptions s
        LEFT JOIN AspNetUsers u ON s.UserId = u.Id
        ORDER BY s.DataCreated DESC
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            return [dict(row._mapping) for row in await result.fetchall()]