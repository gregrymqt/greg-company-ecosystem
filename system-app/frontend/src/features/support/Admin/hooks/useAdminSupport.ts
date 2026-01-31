/**
 * Hook para gerenciamento administrativo de tickets
 * Usa adminSupportService
 */

import { useState, useCallback, useRef } from 'react';
import { adminSupportService } from '../services/support-admin.service';
import type {
  SupportTicketDto,
  SupportTicketStatus,
  SupportFilters
} from '../../shared';
import { AlertService } from '@/shared/services/alert.service';

const DEFAULT_FILTERS: SupportFilters = {
  page: 1,
  pageSize: 10,
  status: undefined,
  searchTerm: undefined
};

export const useAdminSupport = () => {
  const [tickets, setTickets] = useState<SupportTicketDto[]>([]);
  const [currentTicket, setCurrentTicket] = useState<SupportTicketDto | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [filters, setFilters] = useState<SupportFilters>(DEFAULT_FILTERS);
  const [hasMore, setHasMore] = useState<boolean>(true);
  
  // Previne chamadas duplicadas
  const loadingRef = useRef(false);

  /**
   * Busca tickets paginados
   */
  const fetchTickets = useCallback(async (reset = false) => {
    if (loadingRef.current) return;

    loadingRef.current = true;
    setLoading(true);

    try {
      const response = await adminSupportService.getAllTickets(
        reset ? { ...filters, page: 1 } : filters
      );

      if (response.success && response.data) {
        const { items, totalPages, currentPage } = response.data;

        setTickets(prev => reset ? items : [...prev, ...items]);
        setFilters(prev => ({ ...prev, page: currentPage }));
        setHasMore(currentPage < totalPages);
      }
    } catch (err) {
      const errorMessage = (err as Error).message || 'Erro ao carregar tickets.';
      AlertService.error('Erro', errorMessage);
    } finally {
      setLoading(false);
      loadingRef.current = false;
    }
  }, [filters]);

  /**
   * Busca ticket por ID
   */
  const fetchTicketById = useCallback(async (id: string) => {
    setLoading(true);
    setCurrentTicket(null);

    try {
      const response = await adminSupportService.getTicketById(id);
      
      if (response.success && response.data) {
        setCurrentTicket(response.data);
      } else {
        AlertService.notify('Aviso', 'Ticket não encontrado.', 'warning');
      }
    } catch (err) {
      const errorMessage = (err as Error).message || 'Erro ao buscar ticket.';
      AlertService.error('Erro', errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Atualiza status do ticket
   */
  const updateStatus = useCallback(async (id: string, status: SupportTicketStatus) => {
    try {
      const response = await adminSupportService.updateTicketStatus(id, status);

      if (response.success) {
        AlertService.success('Sucesso', response.message || 'Status atualizado!');
        
        // Atualiza o ticket na lista local
        setTickets(prev =>
          prev.map(t => t.id === id ? { ...t, status } : t)
        );

        // Atualiza o ticket atual se for o mesmo
        if (currentTicket && currentTicket.id === id) {
          setCurrentTicket({ ...currentTicket, status });
        }
      }
    } catch (err) {
      const errorMessage = (err as Error).message || 'Erro ao atualizar status.';
      AlertService.error('Erro', errorMessage);
    }
  }, [currentTicket]);

  /**
   * Atualiza filtros e recarrega
   */
  const updateFilters = useCallback((newFilters: Partial<SupportFilters>) => {
    setFilters(prev => ({ ...prev, ...newFilters, page: 1 }));
    setTickets([]);
    setHasMore(true);
  }, []);

  /**
   * Carrega próxima página
   */
  const loadMore = useCallback(() => {
    if (!hasMore || loading) return;
    setFilters(prev => ({ ...prev, page: prev.page + 1 }));
  }, [hasMore, loading]);

  return {
    tickets,
    currentTicket,
    loading,
    hasMore,
    filters,
    fetchTickets,
    fetchTicketById,
    updateStatus,
    updateFilters,
    loadMore
  };
};
