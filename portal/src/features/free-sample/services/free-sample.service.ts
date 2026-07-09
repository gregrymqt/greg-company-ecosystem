import { ApiService } from "../../../shared/services/api.service";
import type { SseStreamPayload } from "../types/free-sample.types";

// Helper para expor os mesmos headers base configurados no seu api.service.ts
const getStreamHeaders = (): Record<string, string> => {
  const headers: Record<string, string> = {
    "ngrok-skip-browser-warning": "true",
    "Accept": "text/event-stream",
  };
  
  // Recupera o token caso a demo passe a exigir identificação posterior
  const token = localStorage.getItem("@GregCompany:Token"); // Alinhado ao seu STORAGE_KEYS.TOKEN
  if (token) {
    headers["Authorization"] = `Bearer ${JSON.parse(token)}`;
  }
  
  return headers;
};

export const FreeSampleService = {
  /**
   * Dispara o processamento inicial enviando as 3 URLs para a fila 'ecommerce_demo'.
   * Consome diretamente o método POST do seu ApiService global.
   */
  triggerDemo: async (urls: string[]): Promise<{ status: string }> => {
    return ApiService.post<{ status: string }, { urls: string[] }>(
      "/v1/demo", 
      { urls }
    );
  },

  /**
   * Conecta ao stream de Server-Sent Events (SSE) do FastAPI utilizando a Fetch API.
   * Garante o repasse de headers customizados e processa o stream linha por linha.
   * * @param onMessage Callback disparado a cada atualização de estado enviada pelo bot.
   * @param onError Callback para capturar falhas críticas de conexão ou decoding.
   * @param signal AbortSignal para fechar a conexão HTTP quando o componente for desmontado.
   */
  streamProgress: async (
    onMessage: (data: SseStreamPayload) => void,
    onError: (error: unknown) => void,
    signal?: AbortSignal
  ): Promise<void> => {
    try {
      const response = await fetch("/api/v1/demo/stream", {
        method: "GET",
        headers: getStreamHeaders(),
        signal,
      });

      if (!response.ok) {
        throw new Error(`Falha ao conectar no stream: ${response.statusText}`);
      }

      const reader = response.body?.getReader();
      const decoder = new TextDecoder("utf-8");

      if (!reader) {
        throw new Error("O corpo da resposta não suporta streaming (ReadableStream nulo).");
      }

      let buffer = "";

      // Loop assíncrono para ler os chunks de dados trafegados pelo gateway
      while (true) {
        const { value, done } = await reader.read();
        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split("\n");

        // O último elemento do split pode ser uma linha incompleta, guardamos no buffer
        buffer = lines.pop() || "";

        for (const line of lines) {
          const cleanedLine = line.trim();
          
          // Convenção SSE: Mensagens de dados começam estritamente com "data: "
          if (cleanedLine.startsWith("data:")) {
            const jsonString = cleanedLine.replace(/^data:\s*/, "");
            
            if (jsonString === "[DONE]") return; // Sinalizador de fim do stream

            try {
              const parsedData: SseStreamPayload = JSON.parse(jsonString);
              onMessage(parsedData);
            } catch (parseError) {
              console.error("Erro ao fazer o parse do evento SSE:", parseError);
            }
          }
        }
      }
    } catch (error) {
      // Se o erro foi um cancelamento intencional (unmount do componente), ignora
      if (error instanceof DOMException && error.name === "AbortError") {
        return;
      }
      onError(error);
    }
  },
};