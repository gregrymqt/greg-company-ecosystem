import { useEffect, useState, useCallback } from "react";
import { socketService } from "@/shared/services/socket.service";
import { AppHubsBIFastAPI } from "@/shared/enums/hub.enums";
import { subscriptionsService } from "../services/subscriptions.service";
import { AlertService } from "@/shared/services/alert.service";
import type {
  SubscriptionSummary,
  SubscriptionWSPayload,
} from "../types/subscriptions.types";
import { ApiError } from "@/shared/services/api.service";

export const useSubscriptionsAnalytics = () => {
  const [summary, setSummary] = useState<SubscriptionSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const loadInitialData = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await subscriptionsService.getSummary();
      setSummary(data);
    } catch (error) {
      if (error instanceof ApiError) {
        console.error(error.message);
        AlertService.error(
          "Erro em Assinaturas",
          error.message,
        );
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const hub = AppHubsBIFastAPI.BI_SUBSCRIPTIONS;

    socketService.connect(hub);

    // Escuta a atualização de KPIs do backend
    socketService.on<SubscriptionWSPayload>(
      hub,
      "SubscriptionKPIsUpdate",
      (payload) => {
        setSummary(payload.summary);

        if (payload.type === "Manual") {
          AlertService.notify(
            "Atualizado",
            "Dados de assinaturas atualizados manualmente.",
            "success",
          );
        } else {
          AlertService.notify(
            "Recorrência",
            "MRR e Churn atualizados em tempo real.",
            "info",
          );
        }
      },
    );

    loadInitialData();

    return () => {
      socketService.off(hub, "SubscriptionKPIsUpdate");
    };
  }, [loadInitialData]);

  const forceUpdate = async () => {
    try {
      // Chama o handler "ForceUpdate" do seu Python
      await socketService.invoke(
        AppHubsBIFastAPI.BI_SUBSCRIPTIONS,
        "ForceUpdate",
        {},
      );
      AlertService.notify(
        "Solicitado",
        "Atualizando KPIs de assinatura...",
        "info",
      );
    } catch(error) {
      if (error instanceof ApiError) {
        console.error("❌ Erro ao solicitar atualização manual:", error);
        AlertService.error("Erro", error.message);
      }
    }
  };

  return { summary, isLoading, forceUpdate, refresh: loadInitialData };
};
