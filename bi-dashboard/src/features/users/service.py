import asyncio
from typing import List
from datetime import datetime

# Infra Imports
from ...core.infrastructure.rows_client import rows_client
from ...features.rows.service import rows_service
from ...core.infrastructure.redis_client import get_or_create_async

# Feature Imports
from .repository import UsersRepository
from .schemas import UserDTO, UserSummaryDTO

class UsersService:
    def __init__(self, repository: UsersRepository):
        self.repository = repository

    # ==================== KPIS (COM CACHE) ====================

    async def get_user_summary(self, use_cache: bool = True) -> UserSummaryDTO:
        """
        Retorna KPIs de Usuários.
        Cache TTL: 300 segundos (5 min)
        """
        cache_key = "users:kpis:summary"

        async def _calculate():
            # O banco faz o trabalho pesado de contar
            raw = await self.repository.get_users_summary_stats()
            
            total = raw.get('TotalUsers', 0)
            confirmed = raw.get('ConfirmedEmails', 0)
            rate = (confirmed / total * 100) if total > 0 else 0.0

            return UserSummaryDTO(
                TotalUsers=total,
                ConfirmedEmails=confirmed,
                ConfirmationRate=round(rate, 2),
                NewUsersLast30Days=raw.get('NewUsersLast30Days', 0)
            )

        if use_cache:
            return await get_or_create_async(cache_key, _calculate, ttl_seconds=300)
        return await _calculate()

    # ==================== LISTAGEM (REAL-TIME) ====================

    async def get_users_list(self, limit: int = 100) -> List[UserDTO]:
        """Busca lista recente (Geralmente sem cache para ver novos cadastros)"""
        raw_users = await self.repository.get_latest_users(limit)
        
        return [
            UserDTO(
                Id=str(row['Id']),
                Name=row['Name'] or "Desconhecido",
                Email=row['Email'],
                EmailConfirmed=bool(row['EmailConfirmed']),
                CreatedAt=row['CreatedAt']
            ) for row in raw_users
        ]

    # ==================== SYNC ROWS (PARALELO) ====================

    async def sync_users_to_rows(self):
        """
        Sincroniza KPIs e Lista de Usuários.
        """
        # 1. Busca KPIs (pode vir do cache) e Lista (banco) em paralelo
        task_summary = self.get_user_summary(use_cache=False) # Force update pro relatório
        task_list = self.get_users_list(limit=50)

        summary, users_list = await asyncio.gather(task_summary, task_list)

        # 2. Formata (CPU Bound)
        payload_kpis = rows_service.build_users_kpis(summary)
        payload_list = rows_service.build_users_list_payload(users_list)

        # 3. Envia para Rows em paralelo (IO Bound)
        await asyncio.gather(
            rows_client.send_data("Users_KPIs!A1", payload_kpis),
            rows_client.send_data("Users_Data!A1", payload_list)
        )

        return {
            "status": "success",
            "message": f"Synced {len(users_list)} users."
        }

def create_users_service() -> UsersService:
    return UsersService(UsersRepository())