// src/features/free-sample/hooks/useFreeSample.ts
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

  // Garante o fechamento automático do canal se o usuário sair da página
  useEffect(() => {
    return () => {
      disconnectStream();
    };
  }, [disconnectStream]);

  /**
   * Dispara o pipeline completo: publicação na fila de demo e escuta via stream centralizado
   */
  const startDemoProcess = useCallback(async (urls: string[]) => {
    const cleanUrls = urls.filter((url) => url.trim() !== "");

    if (cleanUrls.length === 0) return;
    if (cleanUrls.length > 3) {
      setGlobalError("Operação bloqueada: O limite gratuito é de no máximo 3 URLs.");
      return;
    }

    // Reset preventivo de segurança
    disconnectStream();
    setGlobalError(null);
    setIsProcessing(true);

    // Inicializa o Grid visual das URLs em estado pendente
    const initialState: DemoProductItem[] = cleanUrls.map((url) => ({
      url,
      status: "pending",
      progress: 5,
    }));
    setProducts(initialState);

    try {
      // 1. Envia o lote para o endpoint FastAPI
      await FreeSampleService.triggerDemo(cleanUrls);

      // 2. Prepara o token de cancelamento para o stream
      const controller = new AbortController();
      abortControllerRef.current = controller;

      // 3. Abre o canal e intercepta os chunks de dados em tempo real
      await FreeSampleService.streamProgress(
        (payload: SseStreamPayload) => {
          // 🔥 OTIMIZAÇÃO: Tratamento atômico em uma única passagem de render
          setProducts((prevProducts) => {
            const updatedProducts = prevProducts.map((item) =>
              item.url === payload.url
                ? {
                  ...item,
                  status: payload.status,
                  progress: payload.progress,
                  // Se o chunk intermediário omitir dados parciais, segura o cache local do item
                  original: payload.original ?? item.original,
                  enhanced: payload.enhanced ?? item.enhanced,
                  error: payload.error,
                }
                : item
            );

            // Verifica se todas as URLs do lote atingiram um estado terminal
            const allFinished = updatedProducts.every(
              (p) => p.status === "completed" || p.status === "failed"
            );

            if (allFinished) {
              setIsProcessing(false);
            }

            return updatedProducts;
          });
        },
        (streamError) => {
          console.error("Falha na resiliência do stream:", streamError);
          setGlobalError("A conexão com o servidor de atualização falhou.");
          setIsProcessing(false);
        },
        controller.signal
      );

      // 🛡️ REDE DE SEGURANÇA: Se o stream encerrou a leitura do ReadableStream com sucesso 
      // (bateu o [DONE]), mas por oscilação o status das URLs não fechou em completed/failed, 
      // garante o desbloqueio da UI de qualquer forma.
      setIsProcessing(false);

    } catch (error) {
      setIsProcessing(false);

      // Captura tipada herdando as mensagens tratadas no seu ApiService global
      if (error instanceof ApiError) {
        setGlobalError(error.message);
      } else {
        setGlobalError("Ocorreu um erro inesperado ao processar os seus links.");
      }
    }
  }, [disconnectStream]);

  /**
   * Limpa o estado limpando conexões ativas para uma nova simulação
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