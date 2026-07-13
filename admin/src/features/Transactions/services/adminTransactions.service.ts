import { ApiService } from "@/shared/services/api.service";
import type { PaymentItems, TransactionFilters } from '../types/transactions.types';

/**
 * AdminTransactionsService - Área Administrativa
 * Responsável por visualização de pagamentos falhados e gestão de estornos
 */
export const AdminTransactionsService = {
  /**
   * Busca todos os pagamentos (incluindo falhados) para análise administrativa
   * Endpoint: GET /payments/history
   */
  getAllTransactions: async (filters?: TransactionFilters): Promise<PaymentItems[]> => {
    let url = '/payments/history';
    
    if (filters) {
      const params = new URLSearchParams();
      if (filters.status) params.append('status', filters.status);
      if (filters.page) params.append('page', filters.page.toString());
      if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
      
      const queryString = params.toString();
      if (queryString) {
        url += `?${queryString}`;
      }
    }

    return await ApiService.get<PaymentItems[]>(url);
  },

  /**
   * Busca apenas pagamentos falhados (rejected)
   * Útil para dashboards de monitoramento
   */
  getFailedPayments: async (): Promise<PaymentItems[]> => {
    return await AdminTransactionsService.getAllTransactions({ status: 'rejected' });
  },

  /**
   * Busca apenas pagamentos estornados (refunded)
   * Útil para relatórios de chargebacks
   */
  getRefundedPayments: async (): Promise<PaymentItems[]> => {
    return await AdminTransactionsService.getAllTransactions({ status: 'refunded' });
  }
};
