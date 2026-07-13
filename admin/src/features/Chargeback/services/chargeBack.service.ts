import { ApiService } from "@/shared/services/api.service";
import type { ChargebackPaginatedResponse, ChargebackDetail } from '../types/chargeback.types';

const ENDPOINT_BASE = '/admin/chargebacks';

export const ChargebackService = {
  
  /**
   * Busca a lista paginada de chargebacks com filtros.
   */
  getAll: async (
    page: number = 1, 
    searchTerm: string = '', 
    statusFilter: string = ''
  ): Promise<ChargebackPaginatedResponse> => {
    
    // Montagem segura da Query String para o fetch
    const params = new URLSearchParams();
    params.append('page', page.toString());
    
    const cleanSearch = searchTerm?.trim();
    if (cleanSearch) {
      params.append('searchTerm', cleanSearch);
    }
    
    const cleanStatus = statusFilter?.trim();
    if (cleanStatus) {
      params.append('statusFilter', cleanStatus);
    }

    // Chama o seu ApiService.get passando o tipo esperado <T>
    return await ApiService.get<ChargebackPaginatedResponse>(
      `${ENDPOINT_BASE}?${params.toString()}`
    );
  },

  /**
   * Busca os detalhes completos de um chargeback específico.
   */
  getById: async (id: string): Promise<ChargebackDetail> => {
    return await ApiService.get<ChargebackDetail>(
      `${ENDPOINT_BASE}/${id}/details`
    );
  }
};
