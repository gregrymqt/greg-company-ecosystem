import asyncio
from typing import List
from decimal import Decimal

# Infra Nova
from ...core.infrastructure.rows_client import rows_client
from ...features.rows.service import rows_service
from ...core.infrastructure.redis_client import get_or_create_async

from .repository import SubscriptionsRepository
from .schemas import SubscriptionDTO, SubscriptionSummaryDTO

class SubscriptionsService:
    def __init__(self, repository: SubscriptionsRepository):
        self.repository = repository
    
    # ==================== KPIS COM CACHE ====================

    async def get_subscription_summary(self, use_cache: bool = True) -> SubscriptionSummaryDTO:
        cache_key = "subscriptions:kpis:summary"
        
        async def _calculate_summary():
            raw = await self.repository.get_subscriptions_summary()
            
            total = raw.get('TotalSubscriptions', 0)
            active = raw.get('ActiveSubscriptions', 0)
            cancelled = raw.get('CancelledSubscriptions', 0) # SQL já calculou
            paused = raw.get('PausedSubscriptions', 0)       # SQL já calculou
            
            mrr = Decimal(str(raw.get('MonthlyRecurringRevenue', 0) or 0))
            
            # Regra de Churn: (Cancelados / Total) * 100
            # Cuidado: Sua lógica anterior considerava (Total - Active) como churn, 
            # o que incluiria 'Paused' como Churn. Se isso for intencional, ok.
            # Vou manter a lógica padrão de mercado (Cancelados sobre a base).
            churn_rate = 0.0
            if total > 0:
                churn_rate = (cancelled / total) * 100
            
            return SubscriptionSummaryDTO(
                TotalSubscriptions=total,
                ActiveSubscriptions=active,
                PausedSubscriptions=paused,     # Agora o DTO aceita
                CancelledSubscriptions=cancelled, # Agora o DTO aceita
                MonthlyRecurringRevenue=mrr,
                ChurnRate=round(churn_rate, 2)
            )

        if use_cache:
            return await get_or_create_async(cache_key, _calculate_summary, ttl_seconds=120)
        return await _calculate_summary()

    # ==================== LISTAGEM (SEM CACHE) ====================

    async def get_recent_subscriptions(self, limit: int = 50) -> List[SubscriptionDTO]:
        raw_data = await self.repository.get_recent_subscriptions(limit)
        
        subscriptions_list = []
        for row in raw_data:
            dto = SubscriptionDTO(
                Id=str(row['Id']),
                UserId=str(row['UserId']),
                Status=row['Status'],
                CurrentAmount=Decimal(str(row['CurrentAmount'] or 0)),
                CurrentPeriodEndDate=row['CurrentPeriodEndDate'],
                PlanName=row.get('PlanName'),
                # Mapeando os novos campos do DTO
                UserName=row.get('UserName') or "Unknown",
                UserEmail=row.get('UserEmail') or "No Email"
            )
            subscriptions_list.append(dto)
            
        return subscriptions_list

    # ==================== SYNC ROWS (PARALELO) ====================

    async def sync_subscriptions_to_rows(self):
        # 1. Busca dados em paralelo (Scatter)
        # Force cache bypass para garantir dados frescos no relatório
        task_kpis = self.get_subscription_summary(use_cache=False)
        task_list = self.get_recent_subscriptions(limit=100) # Lista maior para o Rows
        
        summary_dto, list_dto = await asyncio.gather(task_kpis, task_list)

        # 2. Formata Payloads (Síncrono/CPU)
        payload_kpis = rows_service.build_subscription_kpis(summary_dto)
        payload_list = rows_service.build_subscriptions_list_payload(list_dto)

        # 3. Envia para o Rows (Paralelo/Gather)
        await asyncio.gather(
            rows_client.send_data("Subscriptions_KPIs!A1", payload_kpis),
            rows_client.send_data("Subscriptions_Data!A1", payload_list)
        )

        return {
            "status": "success",
            "mrr_synced": float(summary_dto.MonthlyRecurringRevenue),
            "rows_count": len(list_dto)
        }

def create_subscriptions_service() -> SubscriptionsService:
    return SubscriptionsService(SubscriptionsRepository())