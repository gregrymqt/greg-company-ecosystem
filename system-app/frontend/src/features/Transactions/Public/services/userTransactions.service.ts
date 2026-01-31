// src/features/Transactions/Public/services/userTransactions.service.ts
import { ApiService } from "@/shared/services/api.service";
import type { PaymentItems } from "../../shared";

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
  requestRefund: async (paymentId: string): Promise<{ message: string }> => {
    return await ApiService.post(`/refunds/${paymentId}`, {}); 
  }
};
