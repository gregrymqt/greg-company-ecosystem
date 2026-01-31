"""
Support Analytics DTOs  
Data Transfer Objects para análises de suporte
Baseado em SupportTicketDocument.cs
"""

from dataclasses import dataclass
from datetime import datetime
from typing import Optional


@dataclass
class TicketDTO:
    """
    DTO para SupportTicketDocument
    Campos baseados em Documents/Models/SupportTicketDocument.cs
    """
    Id: str  # ObjectId convertido para string
    UserId: str
    Context: str
    Explanation: str
    Status: str  # Open, InProgress, Closed
    CreatedAt: datetime


@dataclass
class TicketSummaryDTO:
    """
    DTO para resumo de tickets
    """
    TotalTickets: int
    OpenTickets: int
    InProgressTickets: int
    ClosedTickets: int
    ResolutionRate: float
    AverageResolutionTime: Optional[float] = None


@dataclass
class TicketByContextDTO:
    """
    DTO para tickets agrupados por contexto/categoria
    """
    Context: str
    Count: int
    Percentage: float


@dataclass
class TicketTrendDTO:
    """
    DTO para tendências de tickets ao longo do tempo
    """
    Date: datetime
    OpenedCount: int
    ClosedCount: int
    NetChange: int


@dataclass
class UserSupportMetricsDTO:
    """
    DTO para métricas de suporte por usuário
    """
    UserId: str
    TicketCount: int
    OpenTickets: int
    ClosedTickets: int
    MostCommonContext: str
