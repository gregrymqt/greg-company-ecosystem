"""
Example: Dependency Injection Pattern Usage
Demonstração de como utilizar os serviços com injeção de dependências
"""

from src.data.infrastructure.database import DatabaseConnection
from src.data.infrastructure.mongo_client import MongoConnection
from src.data.repositories.financial_repository import FinancialRepository
from src.data.repositories.support_repository import SupportRepository
from src.services.financial_service import FinancialService
from src.services.subscription_service import SubscriptionService
from src.services.support_service import SupportService


def example_usage():
    """
    Exemplo de uso dos serviços seguindo o padrão DDD
    """
    
    # 1. Inicializar conexões (Singleton)
    db_connection = DatabaseConnection.get_instance()
    mongo_connection = MongoConnection.get_instance()
    
    # 2. Inicializar repositórios
    financial_repo = FinancialRepository(db_connection.get_session())
    support_repo = SupportRepository(mongo_connection.get_database())
    
    # 3. Inicializar serviços com injeção de dependência
    financial_service = FinancialService(financial_repo)
    subscription_service = SubscriptionService(financial_repo)
    support_service = SupportService(support_repo)
    
    # 4. Usar os serviços
    
    # === FINANCIAL SERVICE ===
    print("=== FINANCIAL ANALYTICS ===")
    
    # Resumo de pagamentos
    payment_summary = financial_service.get_payment_summary()
    print(f"Total Payments: {payment_summary.TotalPayments}")
    print(f"Approval Rate: {payment_summary.ApprovalRate}%")
    print(f"Average Ticket: R$ {payment_summary.AvgTicket}")
    
    # Métricas de receita
    revenue_metrics = financial_service.get_revenue_metrics()
    print(f"\nTotal Revenue: R$ {revenue_metrics.TotalRevenue}")
    print(f"Monthly Revenue: R$ {revenue_metrics.MonthlyRevenue}")
    print(f"Top Payment Method: {revenue_metrics.TopPaymentMethod}")
    
    # Chargebacks
    chargeback_summary = financial_service.get_chargeback_summary()
    print(f"\nTotal Chargebacks: {chargeback_summary.TotalChargebacks}")
    print(f"Win Rate: {chargeback_summary.WinRate}%")
    
    # === SUBSCRIPTION SERVICE ===
    print("\n=== SUBSCRIPTION ANALYTICS ===")
    
    # Resumo de assinaturas
    sub_summary = subscription_service.get_subscription_summary()
    print(f"Total Subscriptions: {sub_summary.TotalSubscriptions}")
    print(f"Active: {sub_summary.ActiveSubscriptions}")
    print(f"MRR: R$ {sub_summary.MonthlyRecurringRevenue}")
    print(f"Churn Rate: {sub_summary.ChurnRate}%")
    print(f"Retention Rate: {sub_summary.RetentionRate}%")
    
    # Assinaturas expirando
    expiring_subs = subscription_service.get_expiring_subscriptions(days=7)
    print(f"\nExpiring in 7 days: {len(expiring_subs)}")
    
    # Métricas por plano
    plan_metrics = subscription_service.get_plan_metrics()
    print("\nTop Plans:")
    for plan in plan_metrics[:3]:
        print(f"  - {plan.PlanName}: {plan.ActiveSubscriptions} subs, Market Share: {plan.MarketShare}%")
    
    # Relatório de saúde
    health_report = subscription_service.get_subscription_health_report()
    print(f"\nHealth Report:")
    print(f"  MRR Growth Rate: {health_report['mrr_analysis']['growth_rate']}%")
    
    # === SUPPORT SERVICE ===
    print("\n=== SUPPORT ANALYTICS ===")
    
    # Resumo de tickets
    ticket_summary = support_service.get_ticket_summary()
    print(f"Total Tickets: {ticket_summary.TotalTickets}")
    print(f"Open: {ticket_summary.OpenTickets}")
    print(f"Resolution Rate: {ticket_summary.ResolutionRate}%")
    
    # Tickets por contexto
    by_context = support_service.get_tickets_by_context_analysis()
    print("\nTickets by Context:")
    for ctx in by_context[:5]:
        print(f"  - {ctx.Context}: {ctx.Count} ({ctx.Percentage}%)")
    
    # Tickets críticos (idade)
    age_distribution = support_service.get_open_tickets_age_distribution()
    print("\nOpen Tickets Age Distribution:")
    for age_range, count in age_distribution.items():
        print(f"  - {age_range}: {count}")
    
    # Relatório de saúde
    support_health = support_service.get_support_health_report()
    print(f"\nCritical old tickets (7+ days): {support_health['alerts']['critical_old_tickets']}")
    
    # 5. Fechar conexões
    db_connection.close()
    mongo_connection.close()


if __name__ == "__main__":
    example_usage()
