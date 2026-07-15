import { ApiService } from "@/shared/services/api.service";
import type { PaymentItems, RefundRequest } from "../types/transactions.types";

/**
 * UserTransactionsService - Área do Cliente
 * Responsável por histórico de pagamentos e solicitação de reembolso
 */
export const UserTransactionsService = {
  /**
   * Busca o histórico de pagamentos do usuário autenticado
   * Endpoint: GET /payments/history
   */
  getPaymentHistory: async (): Promise<PaymentItems[]> => {
    return await ApiService.get<PaymentItems[]>('/payments/history');
  },

  /**
   * Solicita reembolso de um pagamento específico
   * Endpoint: POST /refunds/{paymentId}
   */
  requestRefund: async (paymentId: string, request?: RefundRequest): Promise<{ message: string }> => {
    return await ApiService.post(`/refunds/${paymentId}`, request || {}); 
  }
};
