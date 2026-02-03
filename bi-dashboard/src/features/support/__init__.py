"""
Support Feature Module
"""

from .repository import SupportRepository
from .service import SupportService, create_support_service
from .schemas import TicketDTO, TicketSummaryDTO
from .websocket_handlers import setup_support_hub_handlers


__all__ = [
    'SupportRepository',
    'SupportService',
    'create_support_service',
    'TicketDTO',
    'TicketSummaryDTO',
    'setup_support_hub_handlers',
]
