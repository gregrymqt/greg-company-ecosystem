import { ApiService } from "@/shared/services/api.service";
import type { AICredentialsPayload, WebScraperPayload } from "../types/ai-scrapper.type";

export const AiScraperService = {
  saveCredentials: async (payload: AICredentialsPayload): Promise<void> => {
    return await ApiService.post<void, AICredentialsPayload>("/ai/credentials", payload);
  },
  
  startExtraction: async (url: string): Promise<void> => {
    const payload: WebScraperPayload = { url };
    return await ApiService.post<void, WebScraperPayload>("/scraper/extract", payload);
  }
};
