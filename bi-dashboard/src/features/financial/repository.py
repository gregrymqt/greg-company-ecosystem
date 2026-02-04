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

    async def get_payments_summary_optimized(self) -> Dict[str, Any]:
        query = text("""
        SELECT 
            COUNT(*) AS TotalPayments,
            -- Adicionei esta linha para evitar buscar a lista só para contar:
            SUM(CASE WHEN Status = 'approved' THEN 1 ELSE 0 END) AS CountApproved, 
            
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

    async def get_payment_method_stats(self) -> List[Dict[str, Any]]:
        """
        Retorna a distribuição de receita por método de pagamento.
        Ordenado do maior para o menor (Ranking).
        """
        query = text("""
        SELECT 
            Method, 
            SUM(Amount) as TotalAmount,
            COUNT(*) as TransactionCount
        FROM Payments 
        WHERE Status = 'approved'
        GROUP BY Method
        ORDER BY TotalAmount DESC
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            # Retorna lista de dicionários: [{'Method': 'CreditCard', 'TotalAmount': 50000}, ...]
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
        


    async def get_revenue_dashboard_metrics(self) -> Dict[str, Any]:
        """
        Calcula todas as métricas de receita diretamente no SQL.
        Retorna: Total, Mensal (30d), Anual (365d), Ticket Médio.
        """
        query = text("""
        SELECT 
            -- Totais Gerais
            SUM(Amount) as TotalRevenue,
            COUNT(*) as TotalTransactions,
            
            -- Receita Mensal (Últimos 30 dias)
            SUM(CASE WHEN CreatedAt >= DATEADD(day, -30, GETDATE()) THEN Amount ELSE 0 END) as MonthlyRevenue,
            
            -- Receita Anual (Últimos 365 dias)
            SUM(CASE WHEN CreatedAt >= DATEADD(day, -365, GETDATE()) THEN Amount ELSE 0 END) as YearlyRevenue
            
        FROM Payments
        WHERE Status = 'approved'
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            row = await result.fetchone()
            return dict(row._mapping) if row else {}    