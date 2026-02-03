import asyncio
from typing import List
from datetime import datetime

# Infra Nova
from ...core.infrastructure.rows_client import rows_client
from ...features.rows.service import rows_service
from ...core.infrastructure.redis_client import get_or_create_async

from .repository import SupportRepository
from .schemas import TicketSummaryDTO, TicketDTO

class SupportService:
    def __init__(self, repository: SupportRepository):
        self.repository = repository
    
    # ==================== KPIS COM CACHE ====================

    async def get_ticket_summary(self, use_cache: bool = True) -> TicketSummaryDTO:
        """Cache de 2 minutos para contadores de tickets"""
        cache_key = "support:kpis:summary"
        
        async def _calculate():
            raw = await self.repository.get_tickets_summary()
            
            total = raw.get('TotalTickets', 0)
            closed = raw.get('ClosedTickets', 0)
            
            resolution_rate = 0.0
            if total > 0:
                resolution_rate = (closed / total) * 100
            
            return TicketSummaryDTO(
                TotalTickets=total,
                OpenTickets=raw.get('OpenTickets', 0),
                InProgressTickets=raw.get('InProgressTickets', 0),
                ClosedTickets=closed,
                ResolutionRate=round(resolution_rate, 2)
            )

        if use_cache:
            return await get_or_create_async(cache_key, _calculate, ttl_seconds=120)
        return await _calculate()

    # ==================== LISTAGEM (SEM CACHE) ====================

    async def get_recent_tickets(self, limit: int = 50) -> List[TicketDTO]:
        raw_tickets = await self.repository.get_all_tickets(limit=limit)
        
        dto_list = []
        for t in raw_tickets:
            dto_list.append(TicketDTO(
                Id=str(t.get('_id')),
                UserId=t.get('UserId', 'N/A'),
                Context=t.get('Context', 'N/A'),
                Explanation=t.get('Explanation', ''),
                Status=t.get('Status', 'Unknown'),
                # Trata data que pode vir como string ou datetime do Mongo
                CreatedAt=t.get('CreatedAt') if isinstance(t.get('CreatedAt'), datetime) else datetime.now()
            ))
        return dto_list

    # ==================== ROWS SYNC (PARALELO) ====================

    async def sync_support_to_rows(self):
        # Parallel Fetch (Mongo + Cache Bypass)
        task_kpis = self.get_ticket_summary(use_cache=False)
        task_list = self.get_recent_tickets(limit=50)
        
        summary_dto, tickets_list = await asyncio.gather(task_kpis, task_list)

        # Build Payloads
        payload_kpis = rows_service.build_support_kpis(summary_dto)
        payload_list = rows_service.build_support_list_payload(tickets_list)

        # Parallel Send
        await asyncio.gather(
            rows_client.send_data("Support_Dashboard!A1", payload_kpis),
            rows_client.send_data("Support_Data!A1", payload_list)
        )

        return {"status": "success", "tickets_synced": len(tickets_list)}

def create_support_service() -> SupportService:
    return SupportService(SupportRepository())