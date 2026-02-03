from typing import List, Dict, Any, Optional
from datetime import datetime
from sqlalchemy import text
from ...core.infrastructure.database import get_db_session # Helper Async

class FinancialRepository:
    """
    Repository Assíncrono para Operações Financeiras
    Stack: SQLAlchemy Async + aioodbc
    """

    # ==================== PAYMENTS ====================

    async def get_payments_summary(self) -> Dict[str, Any]:
        """Agregação de pagamentos (Non-blocking)"""
        query = text("""
        SELECT 
            COUNT(*) AS TotalPayments,
            SUM(CASE WHEN Status = 'approved' THEN Amount ELSE 0 END) AS TotalApproved,
            SUM(CASE WHEN Status = 'pending' THEN Amount ELSE 0 END) AS TotalPending,
            SUM(CASE WHEN Status = 'cancelled' THEN Amount ELSE 0 END) AS TotalCancelled,
            COUNT(DISTINCT UserId) AS UniqueCustomers,
            AVG(CASE WHEN Status = 'approved' THEN Amount END) AS AvgTicket
        FROM Payments
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            row = await result.fetchone()
            return dict(row._mapping) if row else {}

    async def get_payments_by_status(self, status: str) -> List[Dict[str, Any]]:
        """Busca pagamentos filtrados por status"""
        query = text("""
        SELECT Id, Amount, Status, Method, CreatedAt 
        FROM Payments 
        WHERE Status = :status
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query, {"status": status})
            return [dict(row._mapping) for row in result]

    # ==================== SUBSCRIPTIONS ====================

    async def get_subscriptions_summary(self) -> Dict[str, Any]:
        """Agregação de assinaturas (MRR, Churn)"""
        query = text("""
        SELECT 
            COUNT(*) AS TotalSubscriptions,
            SUM(CASE WHEN Status = 'authorized' THEN 1 ELSE 0 END) AS ActiveSubscriptions,
            SUM(CASE WHEN Status = 'cancelled' THEN 1 ELSE 0 END) AS CancelledSubscriptions,
            SUM(CASE WHEN Status = 'paused' THEN 1 ELSE 0 END) AS PausedSubscriptions,
            SUM(CASE WHEN Status = 'authorized' THEN CurrentAmount ELSE 0 END) AS MonthlyRecurringRevenue
        FROM Subscriptions
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            row = await result.fetchone()
            return dict(row._mapping) if row else {}

    # ==================== CHARGEBACKS ====================

    async def get_chargeback_summary(self) -> Dict[str, Any]:
        """Agregação de riscos"""
        query = text("""
        SELECT 
            COUNT(*) AS TotalChargebacks,
            SUM(Amount) AS TotalAmount,
            SUM(CASE WHEN Status = 0 THEN 1 ELSE 0 END) AS Novo,
            SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS AguardandoEvidencias,
            SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END) AS Ganhamos,
            SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END) AS Perdemos
        FROM Chargeback
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            row = await result.fetchone()
            return dict(row._mapping) if row else {}