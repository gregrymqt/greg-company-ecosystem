import { useState, useEffect, useCallback } from "react";
import { ChargebackService } from '../services/chargeBack.service';
import type { ChargebackPaginatedResponse } from '../types/chargeback.types';
import { ApiError } from "@/shared/services/api.service";

// Estado inicial dos filtros
interface ChargebackFilters {
  page: number;
  searchTerm: string;
  statusFilter: string;
}

export const useChargebacks = () => {
  const [data, setData] = useState<ChargebackPaginatedResponse | null>(null);
  const [loading, setLoading] = useState<boolean>(true); // Começa carregando
  const [error, setError] = useState<string | null>(null);

  // Estado local dos filtros
  const [filters, setFilters] = useState<ChargebackFilters>({
    page: 1,
    searchTerm: "",
    statusFilter: "",
  });
  const [searchInput, setSearchInput] = useState("");

  // Função que chama o Service
  const fetchChargebacks = useCallback(async (isCurrentCheck: () => boolean = () => true) => {
    setLoading(true);
    setError(null);
    try {
      const response = await ChargebackService.getAll(
        filters.page,
        filters.searchTerm,
        filters.statusFilter
      );
      if (isCurrentCheck()) setData(response);
    } catch (err) {
      if (isCurrentCheck() && err instanceof ApiError) {
        setError(err.message);
        console.error(err);
      }
    } finally {
      if (isCurrentCheck()) setLoading(false);
    }
  }, [filters]); // Recria a função se os filtros mudarem

  // Effect: Dispara a busca sempre que os filtros (page/search/status) mudarem
  useEffect(() => {
    let isCurrent = true;
    let timeoutId: ReturnType<typeof setTimeout>;

    if (filters.searchTerm !== searchInput) {
      timeoutId = setTimeout(() => {
        if (isCurrent) {
          setFilters((prev) => ({ ...prev, searchTerm: searchInput, page: 1 }));
        }
      }, 400);
    } else {
      fetchChargebacks(() => isCurrent);
    }

    return () => {
      isCurrent = false;
      if (timeoutId) clearTimeout(timeoutId);
    };
  }, [fetchChargebacks, searchInput, filters.searchTerm]);

  // Helpers para atualizar filtros facilmente no componente visual
  const setPage = (page: number) => setFilters((prev) => ({ ...prev, page }));
  const setSearchTerm = (term: string) => setSearchInput(term);
  const setStatusFilter = (status: string) =>
    setFilters((prev) => ({ ...prev, statusFilter: status, page: 1 }));

  return {
    // Dados
    chargebacks: data?.chargebacks || [],
    pagination: {
      currentPage: data?.currentPage || 1,
      totalPages: data?.totalPages || 1,
      hasPreviousPage: data?.hasPreviousPage || false,
      hasNextPage: data?.hasNextPage || false,
    },
    // Estados de UI
    loading,
    error,
    // Ações para o componente usar
    filters,
    setPage,
    setSearchTerm,
    setStatusFilter,
    refresh: () => fetchChargebacks(() => true), // Caso precise recarregar manualmente
  };
};
