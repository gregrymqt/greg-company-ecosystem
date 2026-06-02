import { BiApiService } from "@/shared/services/api.service";
import type { Subscription } from "@/types/models";
import type { SubscriptionSummary } from "../types/subscriptions.types";

export const subscriptionsService = {
  // Busca o resumo geral (KPIs)
  getSummary: async (): Promise<SubscriptionSummary> => {
    const response = await BiApiService.get<SubscriptionSummary>(
      "/subscriptions/summary",
    );
    return response;
  },

  // Busca lista de assinaturas recentes/ativas
  getList: async (): Promise<Subscription[]> => {
    const response = await BiApiService.get<Subscription[]>(
      "/subscriptions/list",
    );
    return response;
  },

  // Dispara sincronização para Rows.com
  syncToRows: async () => {
    const response = await BiApiService.post<{ message: string }>(
      "/subscriptions/sync-rows",
      {},
    );
    return response;
  },
};
