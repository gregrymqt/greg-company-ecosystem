"""
Claims Service
Service layer para inteligência de Claims
Lógica de negócio para análise de risco e disputas
"""

from typing import List, Dict, Optional, Any
from decimal import Decimal
from datetime import datetime
from .repository import ClaimsRepository
from .schemas import ClaimAnalyticsDTO
from .enums import InternalClaimStatus


class ClaimsService:
    """
    Service para operações de análise de Claims
    Implementa lógica de negócio e cálculos de métricas
    """
    
    def __init__(self, claims_repository: ClaimsRepository):
        """
        Inicializa o service com injeção de dependência
        
        Args:
            claims_repository: Repositório de Claims injetado
        """
        self.claims_repo = claims_repository
    
    # ==================== ANALYTICS ====================
    
    def get_active_claims_analytics(self) -> List[ClaimAnalyticsDTO]:
        """
        Retorna claims ativas em formato de analytics
        Converte dados do repository para DTOs
        
        Returns:
            Lista de ClaimAnalyticsDTO processados
        """
        raw_claims = self.claims_repo.get_active_claims()
        
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
            analytics_list.append(analytics_dto)
        
        return analytics_list
    
    def calculate_revenue_at_risk(self) -> Decimal:
        """
        Calcula o "Faturamento em Risco"
        Soma de todos os pagamentos com claims abertas (não resolvidas)
        
        Returns:
            Valor total em risco (Decimal)
        """
        active_claims = self.claims_repo.get_active_claims()
        
        total_at_risk = Decimal('0.00')
        for claim in active_claims:
            amount = claim.get('AmountAtRisk')
            if amount is not None:
                total_at_risk += Decimal(amount)
        
        return total_at_risk
    
    def get_claims_summary_by_status(self) -> Dict[str, Any]:
        """
        Retorna resumo de volumetria de claims por status
        
        Returns:
            Dicionário com contagens por status e métricas agregadas
        """
        status_counts = self.claims_repo.get_claims_by_status()
        
        summary = {
            'total_claims': 0,
            'total_at_risk': Decimal('0.00'),
            'by_status': {},
            'avg_days_open': 0.0
        }
        
        total_days = 0.0
        total_count = 0
        
        for status_data in status_counts:
            status = status_data['InternalStatus']
            count = status_data['Count']
            amount = Decimal(status_data.get('TotalAmountAtRisk', 0) or 0)
            avg_days = status_data.get('AvgDaysOpen', 0) or 0
            
            summary['by_status'][status] = {
                'count': count,
                'total_amount': float(amount),
                'avg_days_open': float(avg_days)
            }
            
            summary['total_claims'] += count
            summary['total_at_risk'] += amount
            total_days += avg_days * count
            total_count += count
        
        if total_count > 0:
            summary['avg_days_open'] = total_days / total_count
        
        summary['total_at_risk'] = float(summary['total_at_risk'])
        
        return summary
    
    def get_critical_claims(self) -> List[ClaimAnalyticsDTO]:
        """
        Retorna claims críticas (> 30 dias abertas)
        
        Returns:
            Lista de ClaimAnalyticsDTO críticas ordenadas por dias abertos
        """
        all_claims = self.get_active_claims_analytics()
        critical_claims = [claim for claim in all_claims if claim.is_critical]
        
        # Ordenar por dias abertos (decrescente)
        critical_claims.sort(key=lambda x: x.days_open, reverse=True)
        
        return critical_claims
    
    def get_claims_by_type(self) -> Dict[str, int]:
        """
        Agrupa claims por tipo (ClaimType)
        
        Returns:
            Dicionário com contagem por tipo de claim
        """
        active_claims = self.claims_repo.get_active_claims()
        
        type_counts = {}
        for claim in active_claims:
            claim_type = claim['Type']
            type_counts[claim_type] = type_counts.get(claim_type, 0) + 1
        
        return type_counts
    
    def generate_panel_url(self, mp_claim_id: int) -> str:
        """
        Gera URL dinâmica do painel do MercadoPago para uma claim
        
        Args:
            mp_claim_id: ID da claim no MercadoPago
            
        Returns:
            URL completa do painel
        """
        return f"https://www.mercadopago.com.br/developers/panel/notifications/claims/{mp_claim_id}"
    
    # ==================== KPIs ====================
    
    def get_claims_kpis(self) -> Dict[str, Any]:
        """
        Retorna KPIs consolidados de Claims para dashboards
        
        Returns:
            Dicionário com principais indicadores:
            - Total de claims ativas
            - Faturamento em risco
            - Média de dias em aberto
            - Claims críticas (> 30 dias)
            - Taxa de resolução favorável
        """
        active_claims = self.get_active_claims_analytics()
        summary = self.get_claims_summary_by_status()
        
        # Contar claims críticas
        critical_count = sum(1 for claim in active_claims if claim.is_critical)
        
        # Calcular taxa de resolução favorável (ganhamos vs perdemos)
        status_data = summary['by_status']
        won = status_data.get('ResolvidoGanhamos', {}).get('count', 0)
        lost = status_data.get('ResolvidoPerdemos', {}).get('count', 0)
        total_resolved = won + lost
        win_rate = (won / total_resolved * 100) if total_resolved > 0 else 0
        
        return {
            'active_claims_count': len(active_claims),
            'revenue_at_risk': float(self.calculate_revenue_at_risk()),
            'avg_days_open': summary['avg_days_open'],
            'critical_claims_count': critical_count,
            'win_rate_percentage': round(win_rate, 2),
            'total_resolved': total_resolved,
            'won_count': won,
            'lost_count': lost,
            'by_status': summary['by_status']
        }


# ==================== FACTORY ====================

def create_claims_service() -> ClaimsService:
    """
    Factory method para criar instância do ClaimsService com dependências
    
    Returns:
        ClaimsService configurado
    """
    claims_repository = ClaimsRepository()
    return ClaimsService(claims_repository)
