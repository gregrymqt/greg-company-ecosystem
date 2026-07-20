import { ApiService } from "@/shared/services/api.service";
import type { TransactionFilters, AdminPaymentPaginatedResponse } from '../types/transactions.types';

/**
 * AdminTransactionsService - Área Administrativa
 * Responsável por visualização de pagamentos falhados e gestão de estornos
 */
export const AdminTransactionsService = {
  /**
   * Busca todos os pagamentos (incluindo falhados) para análise administrativa
   * Endpoint: GET /payments/admin/all
   */
  getAllTransactions: async (filters?: TransactionFilters): Promise<AdminPaymentPaginatedResponse> => {
    let url = '/payments/admin/all';
    
    if (filters) {
      const params = new URLSearchParams();
      if (filters.status) params.append('status', filters.status);
      if (filters.search) params.append('search', filters.search);
      if (filters.page) params.append('page', filters.page.toString());
      if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
      
      const queryString = params.toString();
      if (queryString) {
        url += `?${queryString}`;
      }
    }

    return await ApiService.get<AdminPaymentPaginatedResponse>(url);
  },

  /**
   * Busca apenas pagamentos falhados (rejected)
   * Útil para dashboards de monitoramento
   */
  getFailedPayments: async (): Promise<AdminPaymentPaginatedResponse> => {
    return await AdminTransactionsService.getAllTransactions({ status: 'rejected', page: 1, pageSize: 10 });
  },

  /**
   * Busca apenas pagamentos estornados (refunded)
   * Útil para relatórios de chargebacks
   */
  getRefundedPayments: async (): Promise<AdminPaymentPaginatedResponse> => {
    return await AdminTransactionsService.getAllTransactions({ status: 'refunded', page: 1, pageSize: 10 });
  }
};
