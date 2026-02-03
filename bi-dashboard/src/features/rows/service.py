from typing import List, Any
from decimal import Decimal
from datetime import datetime

# Importe os DTOs que você já tem (ajuste o caminho do import conforme sua pasta real)
# Assumindo que estão em shared ou dentro de cada feature
from src.features.claims.schemas import ClaimSummaryDTO, ClaimAnalyticsDTO
from src.features.financial.schemas import ChargebackSummaryDTO, PaymentSummaryDTO, RevenueMetricsDTO
from src.features.subscriptions.schemas import SubscriptionDTO, SubscriptionSummaryDTO
from src.features.support.schemas import TicketDTO, TicketSummaryDTO
from src.features.storage.schemas import StorageStatsDTO, FileDetailDTO
from src.features.users.schemas import UserDTO, UserSummaryDTO
from src.features.content.schemas import ContentSummaryDTO, CourseDTO

# Importe os Schemas de Transporte do Rows (que criamos no passo anterior)
from src.features.rows.schemas import RowsUpdateRequest, RowsCellValue

class RowsService:

    # --- MOTOR GENÉRICO (Mantemos igual) ---
    def _create_payload(self, headers: List[str], data_rows: List[List[Any]]) -> RowsUpdateRequest:
        formatted_matrix = []
        # Adiciona Cabeçalho
        formatted_matrix.append([RowsCellValue(value=h) for h in headers])
        
        # Adiciona Dados
        for row in data_rows:
            formatted_cells = []
            for item in row:
                value = item
                if isinstance(item, Decimal):
                    value = float(item)
                elif isinstance(item, datetime):
                    value = item.strftime("%d/%m/%Y") # Formatei para BR
                elif item is None:
                    value = ""
                formatted_cells.append(RowsCellValue(value=value))
            formatted_matrix.append(formatted_cells)

        return RowsUpdateRequest(cells=formatted_matrix)

    # =========================================================================
    # SEÇÃO FINANCEIRO (PAYMENTS & REVENUE)
    # Métodos que formatam dados de pagamentos e métricas de receita.
    # =========================================================================
    def build_financial_kpis(self, dto: PaymentSummaryDTO) -> RowsUpdateRequest:
        """
        Usa o seu PaymentSummaryDTO
        """
        headers = ["KPI Financeiro", "Valor Atual"]
        
        # Transformamos o objeto em linhas para ficar vertical na planilha
        rows = [
            ["Total Aprovado", dto.TotalApproved],
            ["Total Pendente", dto.TotalPending],
            ["Ticket Médio", dto.AvgTicket],
            ["Taxa de Aprovação", f"{dto.ApprovalRate}%"],
            ["Clientes Únicos", dto.UniqueCustomers]
        ]
        
        return self._create_payload(headers, rows)
    
    def build_revenue_metrics(self, dto: RevenueMetricsDTO) -> RowsUpdateRequest:
        """
        Formata métricas de receita (RevenueMetricsDTO)
        """
        headers = ["Métrica de Receita", "Valor"]
        
        # Mapeamento vertical
        rows = [
            ["Receita Total (LifeTime)", dto.TotalRevenue],
            ["Receita Mensal (30d)", dto.MonthlyRevenue],
            ["Receita Anual (365d)", dto.YearlyRevenue],
            ["Total Transações", dto.TotalTransactions],
            ["Ticket Médio Geral", dto.AverageTransactionValue],
            ["Top Método Pagamento", dto.TopPaymentMethod]
        ]
        
        return self._create_payload(headers, rows)

    # =========================================================================
    # SEÇÃO RISCO (CHARGEBACKS)
    # Métodos que formatam dados relacionados a chargebacks.
    # =========================================================================

    def build_chargeback_kpis(self, dto: ChargebackSummaryDTO) -> RowsUpdateRequest:
        """
        Formata resumo de chargebacks (ChargebackSummaryDTO)
        """
        headers = ["Status Chargeback", "Qtd/Valor"]
        
        rows = [
            ["Total Ocorrências", dto.TotalChargebacks],
            ["Valor Retido", dto.TotalAmount],
            ["Taxa de Vitória", f"{dto.WinRate}%"],
            ["Novos (Ação Necessária)", dto.Novo],
            ["Aguardando Evidências", dto.AguardandoEvidencias],
            ["Ganhamos", dto.Ganhamos],
            ["Perdemos", dto.Perdemos]
        ]
        
        return self._create_payload(headers, rows)

    # =========================================================================
    # SEÇÃO CLAIMS (DISPUTAS)
    # Métodos que formatam KPIs e listas detalhadas de disputas.
    # =========================================================================
    
    def build_claims_kpis(self, dto: ClaimSummaryDTO) -> RowsUpdateRequest:
        """
        Usa o seu ClaimSummaryDTO
        """
        headers = ["Métrica de Disputas", "Resultado"]
        
        rows = [
            ["Total Disputas", dto.TotalClaims],
            ["Em Risco (R$)", dto.TotalAmountAtRisk],
            ["Taxa de Vitória", f"{dto.WinRate}%"],
            ["Taxa de Disputa", f"{dto.DisputeRate}%"],
            ["Dias p/ Resolução", dto.AverageResolutionDays or 0]
        ]
        
        return self._create_payload(headers, rows)
    
    def build_critical_claims_list(self, claims: List[ClaimAnalyticsDTO]) -> RowsUpdateRequest:
        """
        Usa uma LISTA do seu ClaimAnalyticsDTO para criar uma tabela detalhada
        """
        headers = ["ID", "Tipo", "Status", "Dias Aberto", "Valor (R$)", "Crítico"]
        
        rows = []
        for c in claims:
            # Mapeia cada campo do seu DTO para uma coluna
            rows.append([
                c.mp_claim_id,
                c.claim_type,
                c.internal_status,
                c.days_open,
                c.amount_at_risk,
                "SIM" if c.is_critical else "NÃO" # Usa a property do seu DTO!
            ])
            
        return self._create_payload(headers, rows)
    
    # =========================================================================
    # SEÇÃO ASSINATURAS (SUBSCRIPTIONS)
    # Métodos que formatam KPIs de MRR e listas de assinantes.
    # =========================================================================
    
    def build_subscription_kpis(self, dto: SubscriptionSummaryDTO) -> RowsUpdateRequest:
        """
        Formata os KPIs de Assinatura (já estava no seu arquivo logError.md)
        """
        headers = ["Métrica de Assinatura", "Valor"]
        
        rows = [
            ["Assinaturas Ativas", dto.ActiveSubscriptions],
            ["MRR (Receita Mensal)", dto.MonthlyRecurringRevenue],
            ["Churn Rate", f"{dto.ChurnRate}%"],
            ["Total Assinaturas", dto.TotalSubscriptions]
        ]
        
        return self._create_payload(headers, rows)

    def build_subscriptions_list_payload(self, subscriptions: List[SubscriptionDTO]) -> RowsUpdateRequest:
        """
        Formata a lista detalhada de assinaturas.
        """
        headers = ["ID", "User ID", "Plano", "Status", "Valor", "Fim do Período"]
        
        rows = []
        for s in subscriptions:
            rows.append([
                s.Id,
                s.UserId,
                s.PlanName or "Padrão",
                s.Status,
                s.CurrentAmount,
                s.CurrentPeriodEndDate # O _create_payload trata datetime automaticamente
            ])
            
        return self._create_payload(headers, rows)

    # =========================================================================
    # SEÇÃO SUPORTE (TICKETS)
    # Métodos que formatam dados de tickets de suporte.
    # =========================================================================
    
    def build_support_kpis(self, dto: TicketSummaryDTO) -> RowsUpdateRequest:
        """
        Usa o seu TicketSummaryDTO
        """
        headers = ["Suporte", "Status"]

        rows = [
            ["Tickets Abertos", dto.OpenTickets],
            ["Em Andamento", dto.InProgressTickets],
            ["Fechados", dto.ClosedTickets],
            ["Taxa de Resolução", f"{dto.ResolutionRate}%"]
        ]

        return self._create_payload(headers, rows)
    
    def build_support_list_payload(self, tickets: List[TicketDTO]) -> RowsUpdateRequest:
        """
        Gera payload para a Lista Detalhada de Tickets.
        Baseado no TicketDTO do arquivo logError.md
        """
        headers = ["ID", "User ID", "Contexto", "Status", "Data Criação", "Explicação"]
        
        rows = []
        for t in tickets:
            rows.append([
                str(t.Id),          # ID do Mongo convertido para string
                t.UserId,
                t.Context,
                t.Status,
                t.CreatedAt,        # O _create_payload vai tratar a data automaticamente
                t.Explanation
            ])
            
        return self._create_payload(headers, rows)
    
    # =========================================================================
    # SEÇÃO ARMAZENAMENTO (STORAGE)
    # Métodos que formatam métricas de uso de disco e arquivos.
    # =========================================================================
        
    def build_storage_kpis(self, dto: StorageStatsDTO) -> RowsUpdateRequest:
        """
        Formata métricas gerais de armazenamento (StorageStatsDTO)
        """
        headers = ["Métrica de Storage", "Valor"]
        
        rows = [
            ["Total de Arquivos", dto.TotalFiles],
            ["Espaço Total Usado (GB)", dto.TotalGB],
            ["Espaço Total Usado (MB)", dto.TotalMB],
            ["Maior Categoria", dto.LargestCategory or "N/A"],
            ["Menor Categoria", dto.SmallestCategory or "N/A"]
        ]
        
        # Adiciona breakdown por categoria nos KPIs também (opcional, mas útil)
        for cat in dto.CategoryBreakdown:
            rows.append([f"Cat: {cat.FeatureCategoria} (%)", f"{cat.PercentageOfTotal}%"])
            rows.append([f"Cat: {cat.FeatureCategoria} (GB)", cat.TotalGB])

        return self._create_payload(headers, rows)

    def build_largest_files_list(self, files: List[FileDetailDTO]) -> RowsUpdateRequest:
        """
        Formata a lista dos maiores arquivos (List[FileDetailDTO])
        """
        headers = ["Nome do Arquivo", "Categoria", "Tamanho (MB)", "Data Criação"]
        
        rows = []
        for f in files:
            rows.append([
                f.FileName,
                f.FeatureCategoria,
                f.SizeMB,
                f.CriadoEm # O _create_payload vai tratar string/data
            ])
            
        return self._create_payload(headers, rows)
    
    # =========================================================================
    # SEÇÃO USUÁRIOS (USERS)
    # Métodos que formatam dados demográficos e listas de usuários.
    # =========================================================================
        
    def build_users_kpis(self, dto: UserSummaryDTO) -> RowsUpdateRequest:
        """
        Formata KPIs de Usuários (Total, Confirmados, Novos).
        """
        headers = ["Métrica de Usuários", "Valor"]
        
        rows = [
            ["Total de Usuários", dto.TotalUsers],
            ["Emails Confirmados", dto.ConfirmedEmails],
            ["Taxa de Confirmação", f"{dto.ConfirmationRate}%"],
            ["Novos (Últimos 30 dias)", dto.NewUsersLast30Days]
        ]
        
        return self._create_payload(headers, rows)

    def build_users_list_payload(self, users: List[UserDTO]) -> RowsUpdateRequest:
        """
        Formata a lista de usuários.
        """
        headers = ["ID", "Nome", "Email", "Confirmado?", "Criado Em"]
        
        rows = []
        for u in users:
            rows.append([
                u.Id,
                u.Name,
                u.Email,
                "SIM" if u.EmailConfirmed else "NÃO",
                u.CreatedAt # O _create_payload trata datetime automaticamente
            ])
            
        return self._create_payload(headers, rows)
    
    # =========================================================================
    # SEÇÃO CONTEÚDO (COURSES & VIDEOS)
    # Métodos que formatam métricas de engajamento com cursos e vídeos.
    # =========================================================================
        
    def build_content_kpis(self, dto: ContentSummaryDTO) -> RowsUpdateRequest:
        """
        Formata KPIs de Conteúdo (Cursos e Vídeos).
        """
        headers = ["Métrica de Conteúdo", "Valor"]
        
        rows = [
            ["Total de Cursos", dto.TotalCourses],
            ["Cursos Ativos", dto.ActiveCourses],
            ["Total de Vídeos (Biblioteca)", dto.TotalVideosLib],
            ["Média Vídeos/Curso", round(dto.AvgVideosPerCourse, 1)]
        ]
        
        return self._create_payload(headers, rows)

    def build_courses_list_payload(self, courses: List[CourseDTO]) -> RowsUpdateRequest:
        """
        Formata a lista detalhada de cursos.
        """
        headers = ["ID", "Nome do Curso", "Status", "Qtd Vídeos"]
        
        rows = []
        for c in courses:
            rows.append([
                c.Id,
                c.Name,
                "ATIVO" if c.IsActive else "INATIVO",
                c.TotalVideos
            ])
            
        return self._create_payload(headers, rows)

# Singleton
rows_service = RowsService()