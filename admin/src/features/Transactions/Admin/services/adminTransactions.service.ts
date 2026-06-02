// src/features/Transactions/Admin/services/adminTransactions.service.ts
import { ApiService } from "@/shared/services/api.service";
import type { PaymentItems } from "../../shared";

/**
 * AdminTransactionsService - Área Administrativa
 * Responsável por visualização de pagamentos falhados e gestão de estornos
 */
export const AdminTransactionsService = {
  /**
   * Busca todos os pagamentos (incluindo falhados) para análise administrativa
   * Endpoint: GET /admin/transactions ou /payments/history (dependendo do backend)
   */
  getAllTransactions: async (): Promise<PaymentItems[]> => {
    // Por enquanto reutiliza o mesmo endpoint, mas em produção deveria ter um endpoint admin específico
    // Exemplo: return await ApiService.get<PaymentItems[]>('/admin/transactions');
    return await ApiService.get<PaymentItems[]>('/payments/history');
  },

  /**
   * Busca apenas pagamentos falhados (rejected/cancelled)
   * Útil para dashboards de monitoramento
   */
  getFailedPayments: async (): Promise<PaymentItems[]> => {
    const allPayments = await AdminTransactionsService.getAllTransactions();
    return allPayments.filter(payment => {
      const status = payment.status?.toLowerCase() || '';
      return status === 'rejected' || status === 'cancelled' || status === 'failed';
    });
  },

  /**
   * Busca apenas pagamentos estornados (refunded)
   * Útil para relatórios de chargebacks
   */
  getRefundedPayments: async (): Promise<PaymentItems[]> => {
    const allPayments = await AdminTransactionsService.getAllTransactions();
    return allPayments.filter(payment => {
      const status = payment.status?.toLowerCase() || '';
      return status === 'refunded' || status === 'partially_refunded';
    });
  }
};
