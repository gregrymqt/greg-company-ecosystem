import type { TicketSummary, Ticket } from "../types/support.types";
import { ApiService } from "@/shared/services/api.service";

export const supportService = {
  // Busca o resumo dos tickets (KPIs)
  getSummary: async (): Promise<TicketSummary> => {
    const response = await ApiService.get<TicketSummary>("/support/summary");
    return response;
  },

  // Busca lista de tickets recentes
  getTickets: async (): Promise<Ticket[]> => {
    const response = await ApiService.get<Ticket[]>("/support/list");
    return response;
  },

  // Dispara sincronização para Rows.com
  syncToRows: async () => {
    const response = await ApiService.post<{ status: string }>("/support/sync-rows", {});
    return response;
  }
};
