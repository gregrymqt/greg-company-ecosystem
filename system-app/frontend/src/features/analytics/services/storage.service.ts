import { BiApiService } from "@/shared/services/api.service";
import type { StorageStats } from "../types/storage.types";

export const storageService = {
  // Busca estatísticas gerais de uso de disco
  getStats: async (): Promise<StorageStats> => {
    const response = await BiApiService.get<StorageStats>("/storage/stats");
    return response;
  },

  // Dispara sincronização do inventário de arquivos para o Rows
  syncToRows: async () => {
    const response = await BiApiService.post<{ message: string }>("/storage/sync-rows", {});
    return response;
  }
};