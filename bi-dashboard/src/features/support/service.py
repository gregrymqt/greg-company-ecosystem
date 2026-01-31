"""
Support Service
Lógica de negócio para análise de tickets
"""

from .repository import SupportRepository
from .schemas import TicketSummaryDTO


class SupportService:
    """Service para análise de suporte"""
    
    def __init__(self, support_repository: SupportRepository):
        self.repository = support_repository
    
    def get_ticket_summary(self) -> TicketSummaryDTO:
        """Retorna resumo com cálculo de taxa de resolução"""
        raw = self.repository.get_tickets_summary()
        
        total = raw.get('TotalTickets', 0)
        open_tickets = raw.get('OpenTickets', 0)
        in_progress = raw.get('InProgressTickets', 0)
        closed = raw.get('ClosedTickets', 0)
        
        resolution_rate = 0.0
        if total > 0:
            resolution_rate = (closed / total) * 100
        
        return TicketSummaryDTO(
            TotalTickets=total,
            OpenTickets=open_tickets,
            InProgressTickets=in_progress,
            ClosedTickets=closed,
            ResolutionRate=round(resolution_rate, 2)
        )


def create_support_service() -> SupportService:
    """Factory para SupportService"""
    return SupportService(SupportRepository())
