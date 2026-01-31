"""
Hub Enums
Enumeração dos Hubs de WebSocket disponíveis
Similar ao AppHubs.ts do frontend
"""

from enum import Enum


class AppHubs(str, Enum):
    """
    Hubs disponíveis no sistema BI
    Equivalente ao AppHubs do frontend
    """
    
    # Analytics Hubs
    CLAIMS = "/hubs/claims"
    FINANCIAL = "/hubs/financial"
    SUBSCRIPTIONS = "/hubs/subscriptions"
    SUPPORT = "/hubs/support"
    
    # General Hub
    NOTIFICATIONS = "/hubs/notifications"
    
    # Admin Hub
    ADMIN = "/hubs/admin"
    
    @classmethod
    def get_hub_name(cls, hub_path: str) -> str:
        """Extrai nome do hub do path"""
        return hub_path.split('/')[-1]
    
    @classmethod
    def list_hubs(cls) -> list:
        """Lista todos os hubs disponíveis"""
        return [hub.value for hub in cls]
