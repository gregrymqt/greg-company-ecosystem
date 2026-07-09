import { useState, useEffect, useRef, useCallback } from "react";
import { FreeSampleService } from "../services/free-sample.service";
import { ApiError } from "../../../shared/services/api.service";
import type { DemoProductItem, SseStreamPayload } from "../types/free-sample.types";

export const useFreeSample = () => {
  const [products, setProducts] = useState<DemoProductItem[]>([]);
  const [isProcessing, setIsProcessing] = useState<boolean>(false);
  const [globalError, setGlobalError] = useState<string | null>(null);

  // Referência persistente para o controlador de aborto do fetch do SSE
  const abortControllerRef = useRef<AbortController | null>(null);

  /**
   * Força a interrupção da conexão HTTP com o SSE no backend Python
   */
  const disconnectStream = useCallback(() => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      abortControllerRef.current = null;
    }
  }, []);

  // Closes open connections automatically if the user leaves the Landing Page
  useEffect(() => {
    return () => {
      disconnectStream();
    };
  }, [disconnectStream]);

  /**
   * Dispara o pipeline completo: validação, publicação no RabbitMQ e escuta em tempo real.
   */
  const startDemoProcess = useCallback(async (urls: string[]) => {
    const cleanUrls = urls.filter((url) => url.trim() !== "");
    
    if (cleanUrls.length === 0) return;
    if (cleanUrls.length > 3) {
      setGlobalError("Operação bloqueada: O limite gratuito é de no máximo 3 URLs.");
      return;
    }

    // Reset de segurança do estado da Feature
    disconnectStream();
    setGlobalError(null);
    setIsProcessing(true);

    // Inicializa o Grid visual das 3 URLs em estado pendente
    const initialState: DemoProductItem[] = cleanUrls.map((url) => ({
      url,
      status: "pending",
      progress: 5,
    }));
    setProducts(initialState);

    try {
      // 1. Envia o lote para a rota FastAPI do ecommerce-bot (/api/v1/demo)
      await FreeSampleService.triggerDemo(cleanUrls);

      // 2. Cria o sinalizador para a conexão de Stream
      const controller = new AbortController();
      abortControllerRef.current = controller;

      // 3. Abre o canal e intercepta os chunks de dados em tempo real
      await FreeSampleService.streamProgress(
        (payload: SseStreamPayload) => {
          // Atualização funcional atômica: evita stale closures por concorrência de eventos
          setProducts((prevProducts) =>
            prevProducts.map((item) =>
              item.url === payload.url
                ? {
                    ...item,
                    status: payload.status,
                    progress: payload.progress,
                    // Se o payload não trouxer os objetos de dados ainda, mantém o cache local
                    original: payload.original ?? item.original,
                    enhanced: payload.enhanced ?? item.enhanced,
                    error: payload.error,
                  }
                : item
            )
          );

          // Verifica se o stream encerrou todas as tarefas pendentes do lote
          setProducts((currentProducts) => {
            const allFinished = currentProducts.every(
              (p) => p.status === "completed" || p.status === "failed"
            );
            if (allFinished) {
              setIsProcessing(false);
            }
            return currentProducts;
          });
        },
        (streamError) => {
          console.error("Falha na resiliência do stream:", streamError);
          setGlobalError("A conexão com o servidor de atualização falhou.");
          setIsProcessing(false);
        },
        controller.signal
      );

    } catch (error) {
      setIsProcessing(false);
      
      // Captura inteligente do erro usando a classe ApiError do seu api.service.ts
      if (error instanceof ApiError) {
        setGlobalError(error.message);
      } else {
        setGlobalError("Ocorreu um erro inesperado ao processar os seus links.");
      }
    }
  }, [disconnectStream]);

  /**
   * Limpa o dashboard permitindo uma nova simulação pelo usuário
   */
  const resetDemoState = useCallback(() => {
    disconnectStream();
    setProducts([]);
    setIsProcessing(false);
    setGlobalError(null);
  }, [disconnectStream]);

  return {
    products,
    isProcessing,
    globalError,
    startDemoProcess,
    resetDemoState,
  };
};