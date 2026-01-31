"""
Claims Repository
Repositório para acesso aos dados de disputas/chargebacks
Cruzamento com Payments para análise de risco
Baseado em ALL_MODELS.txt - Claims
"""

from typing import List, Dict, Optional, Any
from datetime import datetime, timedelta
from sqlalchemy import text
from sqlalchemy.orm import Session
from ...core.infrastructure import get_db_session, get_db_engine
import pandas as pd
from decimal import Decimal


class ClaimsRepository:
    """
    Repository para operações de Claims (disputas/chargebacks)
    Foco em análise de risco e cruzamento com Payments
    """
    
    def __init__(self):
        self.engine = get_db_engine()
    
    # ==================== QUERIES ESPECÍFICAS PARA BI ====================
    
    def get_active_claims(self) -> List[Dict[str, Any]]:
        """
        Busca Claims ativas (status interno diferente de Resolvido)
        JOIN com Payments usando ResourceId para trazer o valor do pagamento em disputa
        
        Returns:
            Lista de claims ativas com:
            - Dados da claim (MpClaimId, Type, CurrentStage, Status, DataCreated)
            - Valor do pagamento em disputa (Amount from Payments)
            - Dias em aberto (calculado)
            - URL do painel do MercadoPago
        """
        query = """
        SELECT 
            c.Id,
            c.MpClaimId,
            c.ResourceId,
            c.Type,
            c.ResourceType,
            c.CurrentStage,
            c.Status AS InternalStatus,
            c.DataCreated,
            c.UserId,
            u.Name AS UserName,
            u.Email AS UserEmail,
            p.Id AS PaymentId,
            p.Amount AS AmountAtRisk,
            p.Status AS PaymentStatus,
            p.ExternalId AS PaymentExternalId,
            DATEDIFF(DAY, c.DataCreated, GETDATE()) AS DaysOpen,
            CONCAT('https://www.mercadopago.com.br/developers/panel/notifications/claims/', c.MpClaimId) AS PanelUrl
        FROM Claims c
        LEFT JOIN Payments p ON c.ResourceId = p.ExternalId
        LEFT JOIN AspNetUsers u ON c.UserId = u.Id
        WHERE c.Status NOT IN ('ResolvidoGanhamos', 'ResolvidoPerdemos')
        ORDER BY c.DataCreated DESC
        """
        
        with get_db_session() as session:
            result = session.execute(text(query))
            return [dict(row._mapping) for row in result]
    
    def get_claims_by_status(self, internal_status: Optional[str] = None) -> List[Dict[str, Any]]:
        """
        Busca Claims agrupadas por status interno para contagem de volumetria
        
        Args:
            internal_status: Status interno específico (Novo, EmAnalise, etc.)
                           Se None, retorna contagem por todos os status
        
        Returns:
            Se internal_status fornecido: Lista de claims daquele status
            Se None: Contagem agrupada por status
        """
        if internal_status:
            # Retorna claims específicas de um status
            query = """
            SELECT 
                c.Id,
                c.MpClaimId,
                c.ResourceId,
                c.Type,
                c.Status AS InternalStatus,
                c.DataCreated,
                p.Amount AS AmountAtRisk,
                DATEDIFF(DAY, c.DataCreated, GETDATE()) AS DaysOpen
            FROM Claims c
            LEFT JOIN Payments p ON c.ResourceId = p.ExternalId
            WHERE c.Status = :status
            ORDER BY c.DataCreated DESC
            """
            
            with get_db_session() as session:
                result = session.execute(text(query), {"status": internal_status})
                return [dict(row._mapping) for row in result]
        else:
            # Retorna contagem por status
            query = """
            SELECT 
                c.Status AS InternalStatus,
                COUNT(*) AS Count,
                SUM(CASE WHEN p.Amount IS NOT NULL THEN p.Amount ELSE 0 END) AS TotalAmountAtRisk,
                AVG(DATEDIFF(DAY, c.DataCreated, GETDATE())) AS AvgDaysOpen
            FROM Claims c
            LEFT JOIN Payments p ON c.ResourceId = p.ExternalId
            GROUP BY c.Status
            ORDER BY Count DESC
            """
            
            with get_db_session() as session:
                result = session.execute(text(query))
                return [dict(row._mapping) for row in result]
    
    def get_claims_dataframe(self) -> pd.DataFrame:
        """
        Retorna todos os claims como DataFrame para análise avançada
        """
        query = """
        SELECT 
            c.Id,
            c.MpClaimId,
            c.ResourceId,
            c.Type,
            c.Status AS InternalStatus,
            c.CurrentStage,
            c.DataCreated,
            c.UserId,
            p.Amount AS AmountAtRisk,
            p.Status AS PaymentStatus,
            DATEDIFF(DAY, c.DataCreated, GETDATE()) AS DaysOpen
        FROM Claims c
        LEFT JOIN Payments p ON c.ResourceId = p.ExternalId
        ORDER BY c.DataCreated DESC
        """
        
        return pd.read_sql(query, self.engine)
