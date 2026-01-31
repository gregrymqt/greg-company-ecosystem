"""
Support Service - Domain Layer
Serviço de análise de suporte seguindo DDD
Transformações e lógica de negócio para Support Tickets (MongoDB)
"""

from typing import List, Dict, Any
from datetime import datetime, timedelta
from ..data.repositories.support_repository import SupportRepository
from ..models.support_dto import (
    TicketDTO,
    TicketSummaryDTO,
    TicketByContextDTO,
    TicketTrendDTO,
    UserSupportMetricsDTO
)


class SupportService:
    """
    Serviço de domínio para análises de suporte
    Recebe SupportRepository via injeção de dependência
    """
    
    def __init__(self, support_repository: SupportRepository):
        """
        Inicializa o serviço com o repositório
        
        Args:
            support_repository: Instância do SupportRepository
        """
        self.repository = support_repository
    
    # ==================== TICKET ANALYTICS ====================
    
    def get_ticket_summary(self) -> TicketSummaryDTO:
        """
        Retorna resumo de tickets com taxa de resolução
        
        Returns:
            TicketSummaryDTO com métricas calculadas
        """
        raw_summary = self.repository.get_tickets_summary()
        
        total_tickets = raw_summary.get('TotalTickets', 0)
        open_tickets = raw_summary.get('OpenTickets', 0)
        in_progress = raw_summary.get('InProgressTickets', 0)
        closed_tickets = raw_summary.get('ClosedTickets', 0)
        
        # Cálculo da taxa de resolução
        resolution_rate = 0.0
        if total_tickets > 0:
            resolution_rate = (closed_tickets / total_tickets) * 100
        
        return TicketSummaryDTO(
            TotalTickets=total_tickets,
            OpenTickets=open_tickets,
            InProgressTickets=in_progress,
            ClosedTickets=closed_tickets,
            ResolutionRate=round(resolution_rate, 2),
            AverageResolutionTime=None  # Requer campo de fechamento no documento
        )
    
    def get_tickets_by_status(self, status: str) -> List[TicketDTO]:
        """
        Retorna tickets filtrados por status
        
        Args:
            status: Status do ticket (Open, InProgress, Closed)
            
        Returns:
            Lista de TicketDTO
        """
        tickets = self.repository.get_tickets_by_status(status)
        
        return [
            TicketDTO(
                Id=ticket['_id'],
                UserId=ticket['UserId'],
                Context=ticket['Context'],
                Explanation=ticket['Explanation'],
                Status=ticket['Status'],
                CreatedAt=ticket['CreatedAt']
            )
            for ticket in tickets
        ]
    
    def get_tickets_by_context_analysis(self) -> List[TicketByContextDTO]:
        """
        Análise de tickets agrupados por contexto/categoria
        
        Returns:
            Lista de TicketByContextDTO com percentuais
        """
        by_context = self.repository.get_tickets_by_context_count()
        
        # Calcula total para percentuais
        total = sum(item['Count'] for item in by_context)
        
        if total == 0:
            return []
        
        return [
            TicketByContextDTO(
                Context=item['Context'],
                Count=item['Count'],
                Percentage=round((item['Count'] / total) * 100, 2)
            )
            for item in by_context
        ]
    
    def get_ticket_trends(self, days: int = 30) -> List[TicketTrendDTO]:
        """
        Análise de tendências de tickets ao longo do tempo
        
        Args:
            days: Número de dias para analisar (padrão: 30)
            
        Returns:
            Lista de TicketTrendDTO com tendências diárias
        """
        end_date = datetime.utcnow()
        start_date = end_date - timedelta(days=days)
        
        tickets = self.repository.get_tickets_created_in_period(start_date, end_date)
        
        # Agrupa por data
        daily_counts = {}
        for ticket in tickets:
            date = ticket['CreatedAt'].date()
            if date not in daily_counts:
                daily_counts[date] = {'opened': 0, 'closed': 0}
            
            daily_counts[date]['opened'] += 1
            if ticket['Status'] == 'Closed':
                daily_counts[date]['closed'] += 1
        
        # Converte para DTOs
        trends = []
        for date, counts in sorted(daily_counts.items()):
            trends.append(
                TicketTrendDTO(
                    Date=datetime.combine(date, datetime.min.time()),
                    OpenedCount=counts['opened'],
                    ClosedCount=counts['closed'],
                    NetChange=counts['opened'] - counts['closed']
                )
            )
        
        return trends
    
    def get_user_support_metrics(self, limit: int = 10) -> List[UserSupportMetricsDTO]:
        """
        Retorna métricas de suporte por usuário (top usuários)
        
        Args:
            limit: Número de usuários a retornar (padrão: 10)
            
        Returns:
            Lista de UserSupportMetricsDTO
        """
        most_active = self.repository.get_most_active_users(limit)
        
        metrics = []
        for user in most_active:
            user_id = user['UserId']
            
            # Busca todos os tickets do usuário
            user_tickets = self.repository.get_tickets_by_user(user_id)
            
            # Calcula métricas
            open_count = sum(1 for t in user_tickets if t['Status'] == 'Open')
            closed_count = sum(1 for t in user_tickets if t['Status'] == 'Closed')
            
            # Contexto mais comum
            contexts = [t['Context'] for t in user_tickets]
            most_common_context = max(set(contexts), key=contexts.count) if contexts else 'N/A'
            
            metrics.append(
                UserSupportMetricsDTO(
                    UserId=user_id,
                    TicketCount=user['TicketCount'],
                    OpenTickets=open_count,
                    ClosedTickets=closed_count,
                    MostCommonContext=most_common_context
                )
            )
        
        return metrics
    
    def get_support_health_report(self) -> Dict[str, Any]:
        """
        Relatório consolidado de saúde do suporte
        
        Returns:
            Dicionário com análise completa
        """
        summary = self.get_ticket_summary()
        by_context = self.get_tickets_by_context_analysis()
        recent_trends = self.get_ticket_trends(7)  # Última semana
        top_users = self.get_user_support_metrics(5)
        
        # Tickets críticos (abertos há mais de 7 dias)
        seven_days_ago = datetime.utcnow() - timedelta(days=7)
        old_open = self.repository.get_tickets_by_status('Open')
        critical_tickets = [
            t for t in old_open 
            if t['CreatedAt'] < seven_days_ago
        ]
        
        return {
            "overview": {
                "total_tickets": summary.TotalTickets,
                "open": summary.OpenTickets,
                "in_progress": summary.InProgressTickets,
                "closed": summary.ClosedTickets,
                "resolution_rate": summary.ResolutionRate
            },
            "by_category": [
                {
                    "context": ctx.Context,
                    "count": ctx.Count,
                    "percentage": ctx.Percentage
                }
                for ctx in by_context[:5]  # Top 5
            ],
            "alerts": {
                "critical_old_tickets": len(critical_tickets),
                "pending_attention": summary.OpenTickets + summary.InProgressTickets
            },
            "trends_7_days": {
                "total_opened": sum(t.OpenedCount for t in recent_trends),
                "total_closed": sum(t.ClosedCount for t in recent_trends),
                "net_change": sum(t.NetChange for t in recent_trends)
            },
            "top_users": [
                {
                    "user_id": user.UserId,
                    "ticket_count": user.TicketCount,
                    "open": user.OpenTickets,
                    "most_common_issue": user.MostCommonContext
                }
                for user in top_users
            ]
        }
    
    def get_context_distribution(self) -> Dict[str, int]:
        """
        Retorna distribuição simples de tickets por contexto
        
        Returns:
            Dicionário {contexto: quantidade}
        """
        by_context = self.repository.get_tickets_by_context_count()
        
        return {
            item['Context']: item['Count']
            for item in by_context
        }
    
    def get_open_tickets_age_distribution(self) -> Dict[str, int]:
        """
        Retorna distribuição de tickets abertos por idade
        
        Returns:
            Dicionário com contagem por faixa de tempo
        """
        open_tickets = self.repository.get_tickets_by_status('Open')
        now = datetime.utcnow()
        
        distribution = {
            '0-24h': 0,
            '1-3 days': 0,
            '3-7 days': 0,
            '7+ days': 0
        }
        
        for ticket in open_tickets:
            age = (now - ticket['CreatedAt']).days
            
            if age == 0:
                distribution['0-24h'] += 1
            elif age <= 3:
                distribution['1-3 days'] += 1
            elif age <= 7:
                distribution['3-7 days'] += 1
            else:
                distribution['7+ days'] += 1
        
        return distribution
