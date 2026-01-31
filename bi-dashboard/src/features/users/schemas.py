"""
Users Feature - Schemas
DTOs para análise de usuários
"""

from dataclasses import dataclass
from datetime import datetime
from typing import Optional


@dataclass
class UserDTO:
    """DTO para AspNetUsers"""
    Id: str
    Name: str
    Email: str
    EmailConfirmed: bool
    CreatedAt: Optional[datetime] = None
