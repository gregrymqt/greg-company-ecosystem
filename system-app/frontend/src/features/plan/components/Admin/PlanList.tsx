import React, { useEffect, useState, useMemo } from "react";

import { ActionMenu } from "@/components/ActionMenu/ActionMenu";
import { type TableColumn, Table } from "@/components/Table/Table";
import { useAdminPlans } from "@/features/Plan/hooks/useAdminPlans";
import type { PlanAdminSummary, PlanAdminDetail } from "@/features/Plan/types/plans.type";
import styles from "../../styles/PlanList.module.scss";

interface PlanListProps {
  onEditRequest: (id: string) => void; // Callback para o pai abrir o Form
}

export const PlanList: React.FC<PlanListProps> = ({ onEditRequest }) => {
  // Hooks e Estados
  const {
    plans,
    pagination,
    loading,
    fetchAdminPlans,
    getPlanById,
    deletePlan,
    currentPlan,
  } = useAdminPlans();

  const [activeTab, setActiveTab] = useState<"list" | "search">("list");
  const [searchId, setSearchId] = useState("");

  // Carrega a listagem inicial ao entrar na aba 'list'
  useEffect(() => {
    if (activeTab === "list") {
      fetchAdminPlans(1, 10); // Página 1, 10 itens por padrão
    }
  }, [activeTab, fetchAdminPlans]);

  // --- Handlers ---

  const handlePageChange = (newPage: number) => {
    if (pagination && newPage >= 1 && newPage <= pagination.totalPages) {
      fetchAdminPlans(newPage, pagination.pageSize);
    }
  };

  const handleSearchById = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!searchId.trim()) return;
    await getPlanById(searchId);
  };

  // --- Colunas da Tabela ---

  // Colunas para a Listagem Geral (Pagination)
  const listColumns: TableColumn<PlanAdminSummary>[] = useMemo(
    () => [
      {
        header: "Nome (MP)",
        accessor: "reason", // CORRIGIDO: O C# manda 'reason'
        width: "30%",
      },
      {
        header: "Valor",
        // CORRIGIDO: O C# manda o objeto auto_recurring
        render: (item) => {
          const amount = item.auto_recurring?.transaction_amount || 0;
          return new Intl.NumberFormat("pt-BR", {
            style: "currency",
            currency: "BRL",
          }).format(amount);
        },
        width: "20%",
      },
      {
        header: "Frequência",
        width: "15%",
        render: (item) => {
          const freq = item.auto_recurring?.frequency;
          const type =
            item.auto_recurring?.frequency_type === "months"
              ? "Mês(es)"
              : "Dia(s)";
          return `${freq} ${type}`;
        },
      },
      {
        header: "Status",
        width: "15%",
        render: (item) => (
          // CORRIGIDO: O status vem como string "active" ou "cancelled"
          <span
            className={`badge ${item.status === "active" ? "active" : "inactive"}`}
          >
            {item.status === "active" ? "Ativo" : "Inativo"}
          </span>
        ),
      },
      {
        header: "Ações",
        width: "100px",
        render: (item) => (
          <ActionMenu
            // ATENÇÃO: Verifique se seu endpoint de GetById espera o ID do MP ou o PublicId (GUID).
            // Se for o ID do MP, use item.id. Se for GUID, use item.external_reference (se você estiver salvando lá).
            onEdit={() => onEditRequest(item.id)}
            onDelete={() =>
              deletePlan(item.id).then((success) => {
                if (success) fetchAdminPlans(pagination?.currentPage || 1);
              })
            }
          />
        ),
      },
    ],
    [pagination, deletePlan, fetchAdminPlans, onEditRequest],
  );

  // Colunas para a Busca por ID (Detalhe Único)
  // Nota: O tipo PlanAdminDetail é diferente do PlanAdminSummary, precisamos adaptar
  const detailColumns: TableColumn<PlanAdminDetail>[] = useMemo(
    () => [
      { header: "ID Público", accessor: "publicId", width: "30%" },
      { header: "Nome", accessor: "name", width: "25%" },
      {
        header: "Valor",
        width: "20%",
        // Formata manualmente pois o EditDTO traz 'number'
        render: (item) =>
          new Intl.NumberFormat("pt-BR", {
            style: "currency",
            currency: "BRL",
          }).format(item.transactionAmount),
      },
      {
        header: "Recorrência",
        width: "15%",
        render: (item) =>
          `${item.frequency} ${item.frequencyType === "months" ? "Mês(es)" : "Dia(s)"}`,
      },
      {
        header: "Ações",
        width: "100px",
        render: (item) => (
          <ActionMenu
            onEdit={() => onEditRequest(item.publicId)}
            onDelete={() =>
              deletePlan(item.publicId).then((success) => {
                if (success) setSearchId(""); // Limpa busca se deletou o item atual
              })
            }
          />
        ),
      },
    ],
    [deletePlan, onEditRequest],
  );

  // --- Render ---

  return (
    <div className={styles.container}>
      {/* 1. Abas de Navegação */}
      <div className={styles.tabsHeader}>
        <button
          className={activeTab === "list" ? styles.active : ""}
          onClick={() => setActiveTab("list")}
        >
          Listar Todos
        </button>
        <button
          className={activeTab === "search" ? styles.active : ""}
          onClick={() => setActiveTab("search")}
        >
          Buscar por ID
        </button>
      </div>

      {/* 2. Conteúdo das Abas */}

      {/* ABA: LISTAGEM PAGINADA */}
      {activeTab === "list" && (
        <>
          <Table<PlanAdminSummary>
            data={plans}
            columns={listColumns}
            isLoading={loading}
            keyExtractor={(item) => item.id}
            emptyMessage="Nenhum plano cadastrado."
          />

          {/* Rodapé de Paginação */}
          {pagination && (
            <div className={styles.paginationControls}>
              <span>
                Página <strong>{pagination.currentPage}</strong> de{" "}
                {pagination.totalPages}
                (Total: {pagination.totalCount})
              </span>
              <div>
                <button
                  disabled={!pagination.hasPreviousPage || loading}
                  onClick={() => handlePageChange(pagination.currentPage - 1)}
                >
                  &lt; Anterior
                </button>
                <button
                  disabled={!pagination.hasNextPage || loading}
                  onClick={() => handlePageChange(pagination.currentPage + 1)}
                  style={{ marginLeft: "0.5rem" }}
                >
                  Próxima &gt;
                </button>
              </div>
            </div>
          )}
        </>
      )}

      {/* ABA: BUSCA POR ID */}
      {activeTab === "search" && (
        <>
          <form className={styles.searchBar} onSubmit={handleSearchById}>
            <input
              type="text"
              placeholder="Cole o ID do plano aqui (UUID)..."
              value={searchId}
              onChange={(e) => setSearchId(e.target.value)}
            />
            <button type="submit" disabled={loading || !searchId}>
              {loading ? "Buscando..." : "Pesquisar"}
            </button>
          </form>

          {/* Exibimos o currentPlan dentro da Tabela (array de 1 item) */}
          <Table<PlanAdminDetail>
            data={currentPlan ? [currentPlan] : []}
            columns={detailColumns}
            isLoading={loading}
            keyExtractor={(item) => item.publicId}
            emptyMessage={
              searchId
                ? "Nenhum plano encontrado com este ID."
                : "Aguardando busca..."
            }
          />
        </>
      )}
    </div>
  );
};
