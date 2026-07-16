import React, { useState } from "react";

import styles from './AdminSubscriptionList.module.scss';
import { type TableColumn, Table } from "@/components/Table/Table";
import type { AdminSubscriptionList as AdminSubscriptionListType } from '../../types/subscriptions.types';

interface AdminSubscriptionListProps {
  subscriptions: AdminSubscriptionListType[];
  loading: boolean;
  onSearch: (query: string) => void;
  onManageClick: (subscriptionId: string) => void;
}

export const AdminSubscriptionList: React.FC<AdminSubscriptionListProps> = ({
  subscriptions,
  loading,
  onSearch,
  onManageClick
}) => {
  // Estado local para o input de busca
  const [queryInput, setQueryInput] = useState("");

  // --- HELPERS DE FORMATAÇÃO (UX) ---

  const formatCurrency = (val?: number, currencyId = "BRL") => {
    if (val === undefined || val === null) return "-";
    return new Intl.NumberFormat("pt-BR", {
      style: "currency",
      currency: currencyId,
    }).format(val);
  };

  const formatDate = (dateStr: string) => {
    if (!dateStr) return "-";
    return new Intl.DateTimeFormat("pt-BR").format(new Date(dateStr));
  };

  const getStatusClass = (status: string) => {
    switch (status) {
      case "authorized":
        return styles.authorized;
      case "paused":
        return styles.paused;
      case "cancelled":
        return styles.cancelled;
      default:
        return styles.default;
    }
  };

  // --- DEFINIÇÃO DAS COLUNAS DA TABELA ---

  const columns: TableColumn<AdminSubscriptionListType>[] = [
    {
      header: "ID / Email",
      width: "25%",
      render: (item) => (
        <div>
          <div style={{ fontWeight: "bold", fontSize: "0.9em" }}>{item.subscriptionId}</div>
          <div style={{ color: "#666", fontSize: "0.85em" }}>
            {item.payerEmail || "Email não inf."}
          </div>
        </div>
      ),
    },
    {
      header: "Plano",
      width: "15%",
      render: (item) => (
        <span style={{ fontWeight: 500 }}>{item.planName}</span>
      )
    },
    {
      header: "Status",
      width: "15%",
      render: (item) => (
        <span
          className={`${styles.statusBadge} ${getStatusClass(item.status || '')}`}
        >
          {item.status}
        </span>
      ),
    },
    {
      header: "Valor",
      width: "15%",
      render: (item) => formatCurrency(item.amount, "BRL"),
    },
    {
      header: "Próx. Pagamento",
      width: "15%",
      render: (item) =>
        item.nextBillingDate ? formatDate(item.nextBillingDate) : "N/A",
    },
    {
      header: "Criado em",
      width: "15%",
      render: (item) => formatDate(item.dateCreated),
    },
    {
      header: "Ações",
      width: "10%",
      render: (item) => (
        <div className={styles.actions}>
          {item.subscriptionId && (
            <button onClick={() => onManageClick(item.subscriptionId!)}>
              Gerenciar
            </button>
          )}
        </div>
      ),
    },
  ];

  // --- HANDLERS ---

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (queryInput.trim()) {
      onSearch(queryInput);
    }
  };

  // Array direto das props
  const tableData = subscriptions || [];

  return (
    <div className={styles.container}>
      <h2>Gerenciamento de Assinaturas</h2>

      {/* Barra de Busca */}
      <form onSubmit={handleSearch} className={styles.searchSection}>
        <input
          type="text"
          placeholder="Cole o ID da assinatura ou Email..."
          value={queryInput}
          onChange={(e) => setQueryInput(e.target.value)}
        />
        <button type="submit" disabled={loading}>
          {loading ? (
            <>Buscando...</>
          ) : (
            <>
              {/* Você pode usar um ícone aqui, ex: FontAwesome */}
              <i className="fas fa-search" style={{ marginRight: 5 }}></i>{" "}
              Buscar
            </>
          )}
        </button>
      </form>

      {/* Tabela Genérica */}
      <Table<AdminSubscriptionListType>
        data={tableData}
        columns={columns}
        isLoading={loading}
        keyExtractor={(item) => item.subscriptionId || Math.random().toString()}
        emptyMessage="Nenhuma assinatura encontrada."
      />
    </div>
  );
};
