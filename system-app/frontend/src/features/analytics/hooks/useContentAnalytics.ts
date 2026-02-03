import { useState, useEffect, useCallback } from "react";
import { AppHubsBIFastAPI } from "@/shared/enums/hub.enums";
import { ApiError } from "@/shared/services/api.service";
import { contentService } from "../services/content.service";
import type {
  ContentSummary,
  ContentWebSocketPayload,
} from "../types/content.types";
import { socketService } from "@/shared/services/socket.service";
import { AlertService } from "@/shared/services/alert.service";

export const useContentAnalytics = () => {
  const [summary, setSummary] = useState<ContentSummary | null>(null);
  const [loading, setLoading] = useState(true);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const data = await contentService.getSummary();
      setSummary(data);
    } catch (error) {
      if (error instanceof ApiError) {
        console.error(error.message);
        AlertService.error("Erro ao carregar", error.message);
      }
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();

    // Conecta ao hub de conteúdo do FastAPI
    socketService.connect(AppHubsBIFastAPI.BI_CONTENT);

    // Escuta o evento definido no seu setup_content_hub_handlers
    socketService.on<ContentWebSocketPayload>(
      AppHubsBIFastAPI.BI_CONTENT,
      "ContentMetricsUpdate",
      (payload) => {
        setSummary(payload.summary);
        if (payload.type === "Manual") {
          AlertService.success("Métricas atualizadas manualmente");
        }
      },
    );

    return () => socketService.disconnect(AppHubsBIFastAPI.BI_CONTENT);
  }, [loadData]);

  const forceUpdate = async () => {
    try {
      await socketService.invoke(AppHubsBIFastAPI.BI_CONTENT, "ForceUpdate", {});
      AlertService.notify(
        "Solicitação Enviada",
        "Atualizando métricas de conteúdo...",
        "success",
      );
    } catch (error) {
      if (error instanceof ApiError) {
        console.error("❌ Erro ao solicitar atualização manual:", error);
        AlertService.error("Erro", error.message);
      }
    }
  };

  return { summary, loading, forceUpdate, refresh: loadData };
};
