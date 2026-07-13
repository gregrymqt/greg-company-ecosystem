import { ApiService } from "@/shared/services/api.service";
import type {
  AdminSubscriptionDetail,
  UpdateSubscriptionValuePayload,
  UpdateSubscriptionStatusPayload,
} from "../types/subscriptions.types";

export const adminSubscriptionService = {
  /**
   * Busca uma assinatura pelo ID (ou query string conforme seu controller).
   * Rota: GET /api/admin/subscriptions/search?query=...
   */
  searchSubscription: async (
    query: string
  ): Promise<AdminSubscriptionDetail> => {
    const params = new URLSearchParams({ query });
    return await ApiService.get<AdminSubscriptionDetail>(`/admin/subscriptions/search?${params.toString()}`);
  },

  /**
   * Atualiza o valor da mensalidade da assinatura.
   * Rota: PUT /api/admin/subscriptions/{id}/value
   */
  updateValue: async (
    id: string,
    amount: number
  ): Promise<AdminSubscriptionDetail> => {
    const payload: UpdateSubscriptionValuePayload = {
      transaction_amount: amount,
      currency_id: "BRL", // Default seguro, já que sua API pede isso
    };

    return await ApiService.put<AdminSubscriptionDetail>(
      `/admin/subscriptions/${id}/value`,
      payload
    );
  },

  /**
   * Atualiza o status (Pausar, Cancelar, Ativar).
   * Rota: PUT /api/admin/subscriptions/{id}/status
   */
  updateStatus: async (
    id: string,
    status: "authorized" | "paused" | "cancelled"
  ): Promise<AdminSubscriptionDetail> => {
    const payload: UpdateSubscriptionStatusPayload = {
      status: status,
    };

    return await ApiService.put<AdminSubscriptionDetail>(
      `/admin/subscriptions/${id}/status`,
      payload
    );
  },
};
