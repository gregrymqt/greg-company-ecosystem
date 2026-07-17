import { ApiService } from "@/shared/services/api.service";
import type { AICredentialsPayload, WebScraperPayload, WebScraperResponse } from "../types/ai-scrapper.type";

export const AiScraperService = {
  saveCredentials: async (payload: AICredentialsPayload): Promise<void> => {
    return await ApiService.post<void, AICredentialsPayload>("/v1/ai/credentials", payload);
  },
  
  startExtraction: async (payload: WebScraperPayload): Promise<WebScraperResponse> => {
    return await ApiService.post<WebScraperResponse, WebScraperPayload>("/v1/scraper/extract", payload);
  }
};
