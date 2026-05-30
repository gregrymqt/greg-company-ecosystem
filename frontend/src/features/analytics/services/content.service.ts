import type { Course } from "@/types/models";
import type { ContentSummary } from "../types/content.types";
import { BiApiService } from "@/shared/services/api.service";


export const contentService = {
  // Busca métricas gerais de conteúdo
  getSummary: async (): Promise<ContentSummary> => {
    const response = await BiApiService.get<ContentSummary>("/content/summary");
    return response;
  },

  // Busca lista de cursos para o dashboard
  getCourses: async (): Promise<Course[]> => {
    const response = await BiApiService.get<Course[]>("/content/list");
    return response;
  },

  // Sincronização com Rows.com (Integration Feature)
  syncToRows: async () => {
    return (await BiApiService.post<{ message: string }>("/content/sync-rows",{}));
  }
};