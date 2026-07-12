// src/features/free-sample/services/free-sample.service.ts
import { ApiService } from "../../../shared/services/api.service";
import type { SseStreamPayload } from "../types/free-sample.types";

export const FreeSampleService = {
  /**
   * Dispara o processamento inicial enviando as 3 URLs para a fila 'ecommerce_demo'.
   */
  triggerDemo: async (urls: string[]): Promise<{ status: string }> => {
    return ApiService.post<{ status: string }, { urls: string[] }>(
      "/v1/demo", 
      { urls }
    );
  },

  /**
   * Conecta ao stream consumindo o motor genérico do ApiService global.
   */
  streamProgress: async (
    onMessage: (data: SseStreamPayload) => void,
    onError: (error: unknown) => void,
    signal?: AbortSignal
  ): Promise<void> => {
    return ApiService.stream(
      "/v1/demo/stream", // Mapeia direto no FastAPI (/api/v1/demo/stream via gateway)
      (rawData) => {
        try {
          const parsedData: SseStreamPayload = JSON.parse(rawData);
          onMessage(parsedData);
        } catch (parseError) {
          console.error("Erro ao converter string do SSE em objeto de domínio:", parseError);
        }
      },
      onError,
      signal
    );
  },
};