import { BiApiService } from "@/shared/services/api.service";
import type { ClaimAnalytics, ClaimByReason, ClaimSummary } from "../types/claims.types";


export const claimsService = {
  // Resumo de KPIs (ClaimSummaryDTO)
  getSummary: async (): Promise<ClaimSummary> => {
    const response = await BiApiService.get<ClaimSummary>("/claims/summary");
    return response;
  },

  // Listagem de Claims com risco (ClaimAnalyticsDTO)
  getAnalyticsList: async (): Promise<ClaimAnalytics[]> => {
    const response = await BiApiService.get<ClaimAnalytics[]>("/claims/active");
    return response;
  },

  // Agrupamento por motivos (ClaimByReasonDTO)
  getClaimsByReason: async (): Promise<ClaimByReason[]> => {
    const response = await BiApiService.get<ClaimByReason[]>("/claims/by-reason");
    return response;
  },

  // Disparo para o Rows.com
  syncToRows: async () => {
    // Adicionamos um corpo vazio ({}) para satisfazer a assinatura do método POST.
    return (await BiApiService.post<{ body: string }>("/claims/sync-rows", {}));
  }
};