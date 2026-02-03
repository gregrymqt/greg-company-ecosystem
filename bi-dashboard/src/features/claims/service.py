import asyncio
from typing import List, Dict, Any
from decimal import Decimal
from datetime import datetime

# Infra Nova
from ...core.infrastructure.rows_client import rows_client
from ...features.rows.service import rows_service
from ...core.infrastructure.redis_client import get_or_create_async # <--- O SEU REDIS

from .schemas import ClaimAnalyticsDTO, ClaimSummaryDTO
from .repository import ClaimsRepository

class ClaimsService:
    def __init__(self, claims_repository: ClaimsRepository):
        self.claims_repo = claims_repository
    
    # ==================== ANALYTICS (Agora com Async) ====================
    
    async def get_active_claims_analytics(self) -> List[ClaimAnalyticsDTO]:
        # Agora damos await no repo
        raw_claims = await self.claims_repo.get_active_claims()
        
        analytics_list = []
        for claim in raw_claims:
            analytics_dto = ClaimAnalyticsDTO(
                id=claim['Id'],
                mp_claim_id=claim['MpClaimId'],
                amount_at_risk=Decimal(claim.get('AmountAtRisk', 0) or 0),
                claim_type=claim['Type'],
                internal_status=claim['InternalStatus'],
                days_open=claim['DaysOpen'],
                panel_url=claim['PanelUrl'],
                resource_id=claim.get('ResourceId'),
                current_stage=claim.get('CurrentStage'),
                user_name=claim.get('UserName'),
                user_email=claim.get('UserEmail'),
                payment_status=claim.get('PaymentStatus'),
                date_created=claim.get('DataCreated')
            )
            analytics_dto.is_critical = (analytics_dto.days_open or 0) > 30 
            analytics_list.append(analytics_dto)
        
        return analytics_list
    
    # ==================== KPIS COM REDIS CACHE ====================
    
    async def get_claims_kpis(self, use_cache: bool = True) -> Dict[str, Any]:
        """
        Retorna KPIs. Usa Redis Cache se use_cache=True.
        """
        cache_key = "claims:kpis:dashboard"
        
        # Esta é a função factory que o Redis vai chamar se der Cache Miss
        async def _calculate_kpis_from_db():
            # Busca dados em paralelo (Scatter-Gather Pattern)
            active_claims_task = self.get_active_claims_analytics()
            status_counts_task = self.claims_repo.get_claims_by_status_counts()
            
            active_claims, status_raw = await asyncio.gather(active_claims_task, status_counts_task)
            
            # Processamento em Memória (Rápido)
            summary = self._process_status_summary(status_raw)
            critical_count = sum(1 for c in active_claims if c.is_critical)
            revenue_risk = sum(c.amount_at_risk for c in active_claims)
            
            # Retorno puro (Dicionário)
            return {
                'active_claims_count': len(active_claims),
                'revenue_at_risk': float(revenue_risk),
                'avg_days_open': summary['avg_days_open'],
                'critical_claims_count': critical_count,
                'win_rate_percentage': summary['win_rate'],
                'total_resolved': summary['total_resolved'],
                'won_count': summary['won'],
                'lost_count': summary['lost'],
                'by_status': summary['by_status_dict']
            }

        if use_cache:
            # A mágica acontece aqui:
            return await get_or_create_async(cache_key, _calculate_kpis_from_db, ttl_seconds=60)
        else:
            return await _calculate_kpis_from_db()

    # Helper interno para processar dados (não precisa ser async, é CPU puro)
    def _process_status_summary(self, status_counts: List[Dict]) -> Dict:
        summary = {
            'by_status_dict': {}, 
            'avg_days_open': 0.0,
            'won': 0, 
            'lost': 0
        }
        total_days = 0.0
        total_count = 0
        
        for row in status_counts:
            status = row['InternalStatus']
            count = row['Count']
            avg = row['AvgDaysOpen'] or 0
            
            summary['by_status_dict'][status] = {
                'count': count,
                'total_amount': float(row['TotalAmountAtRisk'] or 0),
                'avg_days_open': float(avg)
            }
            
            total_days += avg * count
            total_count += count
            
            if status == 'ResolvidoGanhamos': summary['won'] = count
            if status == 'ResolvidoPerdemos': summary['lost'] = count

        if total_count > 0:
            summary['avg_days_open'] = total_days / total_count
            
        summary['total_resolved'] = summary['won'] + summary['lost']
        summary['win_rate'] = (summary['won'] / summary['total_resolved'] * 100) if summary['total_resolved'] > 0 else 0
        
        return summary

    # ==================== ROWS SYNC (Mantido Async) ====================
    
    async def sync_claims_to_rows(self):
        # Busca dados frescos (force cache bypass ou reuse se preferir)
        # Aqui vamos forçar bypass para garantir que o Rows receba o mais recente
        active_claims_list = await self.get_active_claims_analytics()
        kpis_dict = await self.get_claims_kpis(use_cache=False) 
        
        summary_dto = ClaimSummaryDTO(
            TotalClaims=kpis_dict.get('active_claims_count', 0),
            TotalAmountAtRisk=Decimal(kpis_dict.get('revenue_at_risk', 0)),
            WinRate=Decimal(kpis_dict.get('win_rate_percentage', 0)),
            DisputeRate=Decimal(0),
            AverageResolutionDays=int(kpis_dict.get('avg_days_open', 0)),
            OpenedClaims=kpis_dict.get('active_claims_count', 0),
        )

        payload_kpis = rows_service.build_claims_kpis(summary_dto)
        payload_list = rows_service.build_critical_claims_list(active_claims_list)

        await asyncio.gather(
            rows_client.send_data("Dashboard!A1", payload_kpis),
            rows_client.send_data("Claims_Data!A1", payload_list)
        )
        
        return {"status": "success"}

def create_claims_service() -> ClaimsService:
    return ClaimsService(ClaimsRepository())