/**
 * Lista de tickets para administradores
 * Com funcionalidades de filtro e atualização de status
 */

import React, { useEffect, useMemo, useRef } from 'react';
import { useAdminSupport } from '../hooks/useAdminSupport';
import type { SupportTicketDto, SupportTicketStatus } from '../../shared';
import { ActionMenu } from '@/components/ActionMenu/ActionMenu';
import { type TableColumn, Table } from '@/components/Table/Table';
import styles from '../styles/SupportTicketList.module.scss';

export const SupportTicketList: React.FC = () => {
  const {
    tickets,
    loading,
    hasMore,
    fetchTickets,
    updateStatus,
    loadMore
  } = useAdminSupport();

  const observerTarget = useRef<HTMLDivElement>(null);

  // Carrega tickets ao montar
  useEffect(() => {
    if (tickets.length === 0) {
      fetchTickets(true);
    }
  }, [fetchTickets, tickets.length]);

  // Scroll infinito
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasMore && !loading) {
          loadMore();
        }
      },
      { threshold: 1.0 }
    );

    if (observerTarget.current) {
      observer.observe(observerTarget.current);
    }

    return () => observer.disconnect();
  }, [hasMore, loading, loadMore]);

  // Renderiza badge de status
  const renderStatusBadge = (status: SupportTicketStatus) => {
    const statusMap = {
      Open: { label: 'Aberto', className: styles.open },
      InProgress: { label: 'Em Andamento', className: styles.inProgress },
      Closed: { label: 'Fechado', className: styles.closed }
    };

    const { label, className } = statusMap[status];
    return <span className={`${styles.badge} ${className}`}>{label}</span>;
  };

  // Colunas da tabela
  const columns = useMemo<TableColumn<SupportTicketDto>[]>(() => [
    {
      header: 'Data',
      width: '120px',
      render: (item) => new Date(item.createdAt).toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      })
    },
    {
      header: 'Assunto',
      accessor: 'context',
      width: '180px'
    },
    {
      header: 'Descrição',
      render: (item) => (
        <span title={item.explanation}>
          {item.explanation.length > 60
            ? `${item.explanation.substring(0, 60)}...`
            : item.explanation}
        </span>
      )
    },
    {
      header: 'Status',
      width: '140px',
      render: (item) => renderStatusBadge(item.status)
    },
    {
      header: 'Ações',
      width: '100px',
      render: (item) => (
        <ActionMenu>
          {item.status === 'Open' && (
            <button
              className={`${styles.actionItem} ${styles.warning}`}
              onClick={() => updateStatus(item.id, 'InProgress')}
            >
              <i className="fas fa-play-circle"></i> Iniciar Atendimento
            </button>
          )}

          {item.status === 'InProgress' && (
            <button
              className={`${styles.actionItem} ${styles.success}`}
              onClick={() => updateStatus(item.id, 'Closed')}
            >
              <i className="fas fa-check-circle"></i> Finalizar
            </button>
          )}

          {item.status === 'Closed' && (
            <button
              className={styles.actionItem}
              onClick={() => updateStatus(item.id, 'InProgress')}
            >
              <i className="fas fa-undo"></i> Reabrir
            </button>
          )}
        </ActionMenu>
      )
    }
  ], [updateStatus]);

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div>
          <h2>Gerenciamento de Suporte</h2>
          <small className="text-muted">
            Visualize e atenda as solicitações dos usuários
          </small>
        </div>

        <button
          className={styles.refreshBtn}
          onClick={() => fetchTickets(true)}
          disabled={loading}
        >
          <i className={`fas fa-sync-alt ${loading ? 'fa-spin' : ''}`}></i>
          Atualizar
        </button>
      </header>

      <Table<SupportTicketDto>
        data={tickets}
        columns={columns}
        isLoading={loading}
        keyExtractor={(item) => item.id}
        emptyMessage="Nenhum ticket de suporte encontrado."
      />

      {/* Elemento observador para scroll infinito */}
      {hasMore && !loading && (
        <div ref={observerTarget} className={styles.loadMoreTrigger}>
          Carregando mais...
        </div>
      )}
    </div>
  );
};
