import { useEffect, useState, useCallback } from "react";
import { socketService } from "@/shared/services/socket.service";
import { AppHubsBIFastAPI } from "@/shared/enums/hub.enums";
import { supportService } from "../services/support.service";
import { AlertService } from "@/shared/services/alert.service";
import type { TicketSummary, SupportWSPayload } from "../types/support.types";
import { ApiError } from "@/shared/services/api.service";

export const useSupportAnalytics = () => {
  const [summary, setSummary] = useState<TicketSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const loadInitialData = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await supportService.getSummary(); //
      setSummary(data);
    } catch (error) {
      if (error instanceof ApiError) {
        console.error(error.message);
        AlertService.error("Erro em Suporte", error.message);
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const hub = AppHubsBIFastAPI.BI_SUPPORT;

    socketService.connect(hub);

    // Ouve o evento emitido pelo broadcast_support_update ou handle_force_update
    socketService.on<SupportWSPayload>(
      hub,
      "SupportTicketsUpdate",
      (payload) => {
        setSummary(payload.summary);

        if (payload.type === "Manual") {
          AlertService.notify(
            "Suporte Atualizado",
            "Os tickets foram atualizados manualmente.",
            "success",
          );
        } else {
          AlertService.notify(
            "Novo Ticket/Update",
            "Métricas de suporte atualizadas via rede.",
            "info",
          );
        }
      },
    );

    loadInitialData();

    return () => {
      socketService.off(hub, "SupportTicketsUpdate");
    };
  }, [loadInitialData]);

  // Função para disparar o "ForceUpdate" via Hub
  const forceUpdate = async () => {
    try {
      await socketService.invoke(
        AppHubsBIFastAPI.BI_SUPPORT,
        "ForceUpdate",
        {},
      );
      AlertService.notify(
        "Solicitado",
        "Buscando novos dados de suporte...",
        "info",
      );
    } catch (error) {
      if (error instanceof ApiError) {
        console.error(error.message);
        AlertService.error("Erro", error.message);
      }
    }
  };

  return { summary, isLoading, forceUpdate, refresh: loadInitialData };
};
