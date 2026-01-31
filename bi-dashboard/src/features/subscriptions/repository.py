"""
Subscriptions Repository
Queries para assinaturas
"""

from typing import List, Dict, Any
from sqlalchemy import text
from ...core.infrastructure import get_db_session


class SubscriptionsRepository:
    """Repository para assinaturas"""
    
    def get_subscriptions_summary(self) -> Dict[str, Any]:
        """Resumo de assinaturas"""
        query = """
        SELECT 
            COUNT(*) AS TotalSubscriptions,
            SUM(CASE WHEN Status = 'authorized' THEN 1 ELSE 0 END) AS ActiveSubscriptions,
            SUM(CASE WHEN Status = 'authorized' THEN CurrentAmount ELSE 0 END) AS MonthlyRecurringRevenue
        FROM Subscriptions
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            row = result.fetchone()
            return dict(row._mapping) if row else {}
