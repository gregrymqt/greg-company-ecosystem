import { useEffect, useState, useCallback } from "react";
import { socketService } from "@/shared/services/socket.service";
import { AppHubsBIFastAPI } from "@/shared/enums/hub.enums";
import { claimsService } from "../services/claims.service";
import { AlertService } from "@/shared/services/alert.service";
import type {
  ClaimSummary,
  ClaimAnalytics,
  ClaimByReason,
} from "../types/claims.types";
import { ApiError } from "@/shared/services/api.service";

/**
 * Hook para gerenciar dados de analytics de Claims em tempo real.
 * Centraliza busca inicial (REST) e atualizações via WebSocket.
 */
export const useClaimsAnalytics = () => {
  const [summary, setSummary] = useState<ClaimSummary | null>(null);
  const [analyticsList, setAnalyticsList] = useState<ClaimAnalytics[]>([]);
  const [claimsByReason, setClaimsByReason] = useState<ClaimByReason[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Memorizamos o fetch para evitar recriações desnecessárias
  const fetchInitialData = useCallback(async () => {
    try {
      setIsLoading(true);
      // Busca paralela para performance máxima
      const [summaryData, analyticsData, byReasonData] = await Promise.all([
        claimsService.getSummary(),
        claimsService.getAnalyticsList(),
        claimsService.getClaimsByReason(),
      ]);

      setSummary(summaryData);
      setAnalyticsList(analyticsData);
      setClaimsByReason(byReasonData);
    } catch (error) {
      console.error("❌ Erro ao buscar dados iniciais de claims:", error);
      // Feedback visual para o usuário em caso de falha crítica na carga inicial
      AlertService.error(
        "Erro de Conexão",
        "Não foi possível carregar os dados do Dashboard de Claims.",
      );
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const hub = AppHubsBIFastAPI.BI_CLAIMS;

    // 1. Conexão ao Socket do FastAPI
    socketService.connect(hub);

    // 2. Listeners de Tempo Real
    socketService.on<ClaimSummary>(hub, "UpdateSummary", (data) => {
      setSummary(data);
      // Notificação discreta de atualização em tempo real (Toast)
      AlertService.notify(
        "Dashboard Atualizado",
        "Os KPIs de Claims foram atualizados.",
        "info",
      );
    });

    socketService.on<ClaimAnalytics[]>(hub, "UpdateAnalyticsList", (data) =>
      setAnalyticsList(data),
    );
    socketService.on<ClaimByReason[]>(hub, "UpdateClaimsByReason", (data) =>
      setClaimsByReason(data),
    );

    // 3. Carga Inicial
    fetchInitialData();

    return () => {
      socketService.off(hub, "UpdateSummary");
      socketService.off(hub, "UpdateAnalyticsList");
      socketService.off(hub, "UpdateClaimsByReason");
    };
  }, [fetchInitialData]);

  /**
   * Dispara um refresh manual no backend via WebSocket
   */
  const forceUpdate = async () => {
    try {
      await socketService.invoke(AppHubsBIFastAPI.BI_CLAIMS, "ForceUpdate", {});
      // Usamos o Toast para não interromper o fluxo do usuário
      AlertService.notify(
        "Solicitação Enviada",
        "Atualizando dados de claims...",
        "success",
      );
    } catch (error) {
      if (error instanceof ApiError) {
        console.error("❌ Erro ao solicitar atualização manual:", error);
        AlertService.error("Erro", error.message);
        return;
      }
    }
  };

  return {
    summary,
    analyticsList,
    claimsByReason,
    isLoading,
    forceUpdate,
    refresh: fetchInitialData,
  };
};
