"""
Support Feature - Schemas
DTOs para análise de suporte
"""

from dataclasses import dataclass
from datetime import datetime
from typing import Optional


@dataclass
class TicketDTO:
    """DTO para SupportTicketDocument"""
    Id: str
    UserId: str
    Context: str
    Explanation: str
    Status: str
    CreatedAt: datetime


@dataclass
class TicketSummaryDTO:
    """DTO para resumo de tickets"""
    TotalTickets: int
    OpenTickets: int
    InProgressTickets: int
    ClosedTickets: int
    ResolutionRate: float
    AverageResolutionTime: Optional[float] = None
