import type { TicketSummary, Ticket } from "../types/support.types";
import { BiApiService } from "@/shared/services/api.service";

export const supportService = {
  // Busca o resumo dos tickets (KPIs)
  getSummary: async (): Promise<TicketSummary> => {
    const response = await BiApiService.get<TicketSummary>("/support/summary");
    return response;
  },

  // Busca lista de tickets recentes
  getTickets: async (): Promise<Ticket[]> => {
    const response = await BiApiService.get<Ticket[]>("/support/list");
    return response;
  },

  // Dispara sincronização para Rows.com
  syncToRows: async () => {
    const response = await BiApiService.post<{ status: string }>("/support/sync-rows", {});
    return response;
  }
};