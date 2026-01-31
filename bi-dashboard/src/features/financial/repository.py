"""
Financial Repository
Repositório para acesso aos dados financeiros (Payments, Subscriptions, Plans)
Baseado nas tabelas do ApiDbContext.cs e ALL_MODELS.txt
"""

from typing import List, Dict, Optional, Any
from datetime import datetime, timedelta
from sqlalchemy import text
from ...core.infrastructure import get_db_session, get_db_engine
import pandas as pd


class FinancialRepository:
    """
    Repository para operações financeiras
    Foca em queries de leitura (BI/Analytics)
    """
    
    def __init__(self):
        self.engine = get_db_engine()
    
    # ==================== PAYMENTS ====================
    
    def get_all_payments(self, limit: Optional[int] = None) -> List[Dict[str, Any]]:
        """
        Busca todos os pagamentos do sistema
        Campos baseados em ALL_MODELS.txt - Payments
        """
        query = """
        SELECT 
            p.Id,
            p.ExternalId,
            p.UserId,
            p.Status,
            p.PayerEmail,
            p.Method,
            p.Installments,
            p.DateApproved,
            p.LastFourDigits,
            p.CustomerCpf,
            p.Amount,
            p.Description,
            p.SubscriptionId,
            p.CreatedAt,
            p.UpdatedAt,
            u.Name AS UserName,
            u.Email AS UserEmail
        FROM Payments p
        LEFT JOIN AspNetUsers u ON p.UserId = u.Id
        ORDER BY p.CreatedAt DESC
        """
        
        if limit:
            query += f" OFFSET 0 ROWS FETCH NEXT {limit} ROWS ONLY"
        
        with get_db_session() as session:
            result = session.execute(text(query))
            return [dict(row._mapping) for row in result]
    
    def get_payments_by_status(self, status: str) -> List[Dict[str, Any]]:
        """
        Busca pagamentos por status (approved, pending, cancelled, etc)
        """
        query = """
        SELECT 
            p.Id,
            p.ExternalId,
            p.Amount,
            p.Status,
            p.Method,
            p.CreatedAt,
            p.PayerEmail
        FROM Payments p
        WHERE p.Status = :status
        ORDER BY p.CreatedAt DESC
        """
        
        with get_db_session() as session:
            result = session.execute(text(query), {"status": status})
            return [dict(row._mapping) for row in result]
    
    def get_payments_summary(self) -> Dict[str, Any]:
        """
        Retorna resumo financeiro dos pagamentos
        """
        query = """
        SELECT 
            COUNT(*) AS TotalPayments,
            SUM(CASE WHEN Status = 'approved' THEN Amount ELSE 0 END) AS TotalApproved,
            SUM(CASE WHEN Status = 'pending' THEN Amount ELSE 0 END) AS TotalPending,
            SUM(CASE WHEN Status = 'cancelled' THEN Amount ELSE 0 END) AS TotalCancelled,
            COUNT(DISTINCT UserId) AS UniqueCustomers,
            AVG(CASE WHEN Status = 'approved' THEN Amount END) AS AvgTicket
        FROM Payments
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            row = result.fetchone()
            return dict(row._mapping) if row else {}
    
    def get_payments_by_period(self, start_date: datetime, end_date: datetime) -> pd.DataFrame:
        """
        Retorna DataFrame com pagamentos em um período
        Útil para análise com pandas
        """
        query = """
        SELECT 
            p.*,
            u.Name AS UserName
        FROM Payments p
        LEFT JOIN AspNetUsers u ON p.UserId = u.Id
        WHERE p.CreatedAt BETWEEN :start_date AND :end_date
        ORDER BY p.CreatedAt DESC
        """
        
        return pd.read_sql(
            query,
            self.engine,
            params={"start_date": start_date, "end_date": end_date}
        )
    
    # ==================== SUBSCRIPTIONS ====================
    
    def get_all_subscriptions(self, limit: Optional[int] = None) -> List[Dict[str, Any]]:
        """
        Busca todas as assinaturas
        Campos baseados em ALL_MODELS.txt - Subscription
        """
        query = """
        SELECT 
            s.Id,
            s.ExternalId,
            s.UserId,
            s.Status,
            s.PlanId,
            s.CurrentAmount,
            s.CurrentPeriodStartDate,
            s.CurrentPeriodEndDate,
            s.LastFourCardDigits,
            s.PayerEmail,
            s.PaymentMethodId,
            s.CreatedAt,
            s.UpdatedAt,
            p.Name AS PlanName,
            p.TransactionAmount AS PlanAmount,
            u.Name AS UserName,
            u.Email AS UserEmail
        FROM Subscriptions s
        LEFT JOIN Plans p ON s.PlanId = p.Id
        LEFT JOIN AspNetUsers u ON s.UserId = u.Id
        ORDER BY s.CreatedAt DESC
        """
        
        if limit:
            query += f" OFFSET 0 ROWS FETCH NEXT {limit} ROWS ONLY"
        
        with get_db_session() as session:
            result = session.execute(text(query))
            return [dict(row._mapping) for row in result]
    
    def get_active_subscriptions(self) -> List[Dict[str, Any]]:
        """
        Busca assinaturas ativas (status = 'authorized')
        """
        query = """
        SELECT 
            s.Id,
            s.UserId,
            s.Status,
            s.CurrentPeriodEndDate,
            s.CurrentAmount,
            p.Name AS PlanName,
            u.Name AS UserName,
            u.Email AS UserEmail
        FROM Subscriptions s
        LEFT JOIN Plans p ON s.PlanId = p.Id
        LEFT JOIN AspNetUsers u ON s.UserId = u.Id
        WHERE s.Status = 'authorized'
        ORDER BY s.CurrentPeriodEndDate ASC
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            return [dict(row._mapping) for row in result]
    
    def get_subscriptions_summary(self) -> Dict[str, Any]:
        """
        Retorna resumo de assinaturas (MRR, churn, etc)
        """
        query = """
        SELECT 
            COUNT(*) AS TotalSubscriptions,
            SUM(CASE WHEN Status = 'authorized' THEN 1 ELSE 0 END) AS ActiveSubscriptions,
            SUM(CASE WHEN Status = 'cancelled' THEN 1 ELSE 0 END) AS CancelledSubscriptions,
            SUM(CASE WHEN Status = 'paused' THEN 1 ELSE 0 END) AS PausedSubscriptions,
            SUM(CASE WHEN Status = 'authorized' THEN CurrentAmount ELSE 0 END) AS MonthlyRecurringRevenue
        FROM Subscriptions
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            row = result.fetchone()
            return dict(row._mapping) if row else {}
    
    # ==================== CHARGEBACKS ====================
    
    def get_all_chargebacks(self) -> List[Dict[str, Any]]:
        """
        Busca todos os chargebacks
        Campos baseados em ALL_MODELS.txt - Chargeback
        """
        query = """
        SELECT 
            c.Id,
            c.ChargebackId,
            c.PaymentId,
            c.UserId,
            c.Status,
            c.Amount,
            c.CreatedAt,
            c.InternalNotes,
            u.Name AS UserName,
            u.Email AS UserEmail
        FROM Chargeback c
        LEFT JOIN AspNetUsers u ON c.UserId = u.Id
        ORDER BY c.CreatedAt DESC
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            return [dict(row._mapping) for row in result]
    
    def get_chargeback_summary(self) -> Dict[str, Any]:
        """
        Retorna resumo de chargebacks
        """
        query = """
        SELECT 
            COUNT(*) AS TotalChargebacks,
            SUM(Amount) AS TotalAmount,
            SUM(CASE WHEN Status = 0 THEN 1 ELSE 0 END) AS Novo,
            SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS AguardandoEvidencias,
            SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END) AS Ganhamos,
            SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END) AS Perdemos
        FROM Chargeback
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            row = result.fetchone()
            return dict(row._mapping) if row else {}
