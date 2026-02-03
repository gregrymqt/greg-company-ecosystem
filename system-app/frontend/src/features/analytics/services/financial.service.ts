import { BiApiService } from "@/shared/services/api.service";
import type { PaymentSummary, RevenueMetrics, ChargebackSummary } from "../types/financial.types";


export const financialService = {
  getPaymentSummary: async () => {
    const response = await BiApiService.get<PaymentSummary>("/financial/summary");
    return response;
  },
  getRevenueMetrics: async () => {
    const response = await BiApiService.get<RevenueMetrics>("/financial/revenue");
    return response;
  },
  getChargebackSummary: async () => {
    const response = await BiApiService.get<ChargebackSummary>("/financial/chargebacks/summary");
    return response;
  },
  syncToRows: async () => {
    const response = await BiApiService.post<{ message: string }>("/financial/sync-rows", {});
    return response;
  }
};