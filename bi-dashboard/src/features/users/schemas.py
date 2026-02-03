from pydantic import BaseModel
from datetime import datetime
from typing import Optional

class UserDTO(BaseModel):
    """Representação de um usuário"""
    Id: str
    Name: str
    Email: str
    EmailConfirmed: bool
    CreatedAt: Optional[datetime] = None

class UserSummaryDTO(BaseModel):
    """KPIs consolidados de usuários"""
    TotalUsers: int
    ConfirmedEmails: int
    ConfirmationRate: float
    NewUsersLast30Days: int