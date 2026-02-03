from typing import List, Dict, Optional, Any
from sqlalchemy import text
from ...core.infrastructure.database import get_db_session # Seu novo helper async
# Se precisar de logging
import logging

logger = logging.getLogger(__name__)

class ClaimsRepository:
    """
    Repository Assíncrono para Claims
    Stack: SQLAlchemy Async + aioodbc
    """
    
    # ==================== QUERIES ASSÍNCRONAS ====================
    
    async def get_active_claims(self) -> List[Dict[str, Any]]:
        """
        Busca Claims ativas de forma não bloqueante.
        """
        query = text("""
        SELECT 
            c.Id, c.MpClaimId, c.ResourceId, c.Type, c.ResourceType, 
            c.CurrentStage, c.Status AS InternalStatus, c.DataCreated, 
            c.UserId, u.Name AS UserName, u.Email AS UserEmail,
            p.Id AS PaymentId, p.Amount AS AmountAtRisk, 
            p.Status AS PaymentStatus, p.ExternalId AS PaymentExternalId,
            DATEDIFF(DAY, c.DataCreated, GETDATE()) AS DaysOpen,
            CONCAT('https://www.mercadopago.com.br/developers/panel/notifications/claims/', c.MpClaimId) AS PanelUrl
        FROM Claims c
        LEFT JOIN Payments p ON c.ResourceId = p.ExternalId
        LEFT JOIN AspNetUsers u ON c.UserId = u.Id
        WHERE c.Status NOT IN ('ResolvidoGanhamos', 'ResolvidoPerdemos')
        ORDER BY c.DataCreated DESC
        """)
        
        # O "async with" aqui garante que a conexão abre e fecha corretamente sem travar
        async with get_db_session() as session:
            result = await session.execute(query)
            return [dict(row._mapping) for row in result]
    
    async def get_claims_by_status_counts(self) -> List[Dict[str, Any]]:
        """
        Retorna a contagem agrupada para os KPIs.
        """
        query = text("""
        SELECT 
            c.Status AS InternalStatus,
            COUNT(*) AS Count,
            SUM(CASE WHEN p.Amount IS NOT NULL THEN p.Amount ELSE 0 END) AS TotalAmountAtRisk,
            AVG(DATEDIFF(DAY, c.DataCreated, GETDATE())) AS AvgDaysOpen
        FROM Claims c
        LEFT JOIN Payments p ON c.ResourceId = p.ExternalId
        GROUP BY c.Status
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query)
            return [dict(row._mapping) for row in result]

    async def get_claims_details_by_status(self, internal_status: str) -> List[Dict[str, Any]]:
        """
        Busca detalhes filtrados por status.
        """
        query = text("""
        SELECT 
            c.Id, c.MpClaimId, c.ResourceId, c.Type, 
            c.Status AS InternalStatus, c.DataCreated, 
            p.Amount AS AmountAtRisk,
            DATEDIFF(DAY, c.DataCreated, GETDATE()) AS DaysOpen
        FROM Claims c
        LEFT JOIN Payments p ON c.ResourceId = p.ExternalId
        WHERE c.Status = :status
        ORDER BY c.DataCreated DESC
        """)
        
        async with get_db_session() as session:
            result = await session.execute(query, {"status": internal_status})
            return [dict(row._mapping) for row in result]