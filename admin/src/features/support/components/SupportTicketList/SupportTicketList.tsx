/**
 * Lista de tickets para administradores
 * Com funcionalidades de filtro e atualização de status
 */

import React, { useState, useEffect, useMemo, useRef } from 'react';
import { useAdminSupport } from '../../hooks/useAdminSupport';
import type { SupportTicketDto, SupportTicketStatus } from '../../types/support.types';
import { ActionMenu } from '@/components/ActionMenu/ActionMenu';
import { type TableColumn, Table } from '@/components/Table/Table';
import styles from './SupportTicketList.module.scss';

export const SupportTicketList: React.FC = () => {
  const {
    tickets,
    loading,
    hasMore,
    filters,
    fetchTickets,
    fetchTicketById,
    currentTicket,
    updateStatus,
    updateFilters,
    loadMore
  } = useAdminSupport();

  const observerTarget = useRef<HTMLDivElement>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [localSearch, setLocalSearch] = useState(filters.searchTerm || '');

  // Debounce para busca
  useEffect(() => {
    const handler = setTimeout(() => {
      if (localSearch !== filters.searchTerm) {
        updateFilters({ searchTerm: localSearch || undefined });
      }
    }, 500);
    return () => clearTimeout(handler);
  }, [localSearch, filters.searchTerm, updateFilters]);

  // Carrega tickets ao montar
  useEffect(() => {
    fetchTickets(true);
  }, [fetchTickets]);

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
    const statusMap: Record<SupportTicketStatus, { label: string; className: string }> = {
      open: { label: 'Aberto', className: styles.open },
      in_progress: { label: 'Em Andamento', className: styles.inProgress },
      resolved: { label: 'Resolvido', className: styles.success },
      closed: { label: 'Fechado', className: styles.closed }
    };

    const { label, className } = statusMap[status] || { label: status, className: '' };
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
      accessor: 'title',
      width: '180px'
    },
    {
      header: 'Categoria',
      render: (item) => <span>{item.category}</span>
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
          <button
            className={styles.actionItem}
            onClick={() => {
              fetchTicketById(item.id);
              setIsModalOpen(true);
            }}
          >
            <i className="fas fa-eye"></i> Visualizar Detalhes
          </button>

          {item.status === 'open' && (
            <button
              className={`${styles.actionItem} ${styles.warning}`}
              onClick={() => updateStatus(item.id, 'in_progress')}
            >
              <i className="fas fa-play-circle"></i> Iniciar Atendimento
            </button>
          )}

          {item.status === 'in_progress' && (
            <button
              className={`${styles.actionItem} ${styles.success}`}
              onClick={() => updateStatus(item.id, 'closed')}
            >
              <i className="fas fa-check-circle"></i> Finalizar
            </button>
          )}

          {item.status === 'closed' && (
            <button
              className={styles.actionItem}
              onClick={() => updateStatus(item.id, 'in_progress')}
            >
              <i className="fas fa-undo"></i> Reabrir
            </button>
          )}
        </ActionMenu>
      )
    }
  ], [fetchTicketById, updateStatus]);

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div>
          <h2>Gerenciamento de Suporte</h2>
          <small className="text-muted">
            Visualize e atenda as solicitações dos usuários
          </small>
        </div>

        <div className={styles.filterBar}>
          <input
            type="text"
            className={styles.searchInput}
            placeholder="Buscar assunto..."
            value={localSearch}
            onChange={(e) => setLocalSearch(e.target.value)}
          />
          <select
            className={styles.statusSelect}
            value={filters.status || ''}
            onChange={(e) => updateFilters({ status: (e.target.value as SupportTicketStatus) || undefined })}
          >
            <option value="">Todos os Status</option>
            <option value="open">Aberto</option>
            <option value="in_progress">Em Andamento</option>
            <option value="resolved">Resolvido</option>
            <option value="closed">Fechado</option>
          </select>
          <button
            className={styles.refreshBtn}
            onClick={() => fetchTickets(true)}
            disabled={loading}
          >
            <i className={`fas fa-sync-alt ${loading ? 'fa-spin' : ''}`}></i>
            Atualizar
          </button>
        </div>
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

      {/* Modal de Detalhes */}
      {isModalOpen && currentTicket && (
        <div className={styles.modalOverlay} onClick={() => setIsModalOpen(false)}>
          <div className={styles.modalContent} onClick={(e) => e.stopPropagation()}>
            <h3>Detalhes do Ticket</h3>
            <p><strong>ID:</strong> {currentTicket.id}</p>
            <p><strong>ID do Usuário:</strong> {currentTicket.userId}</p>
            <p><strong>Título:</strong> {currentTicket.title}</p>
            <p><strong>Categoria:</strong> {currentTicket.category}</p>
            <p><strong>Prioridade:</strong> {currentTicket.priority}</p>
            <p><strong>Data de Criação:</strong> {new Date(currentTicket.createdAt).toLocaleString('pt-BR')}</p>
            
            <button className={styles.closeBtn} onClick={() => setIsModalOpen(false)}>
              Fechar
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
