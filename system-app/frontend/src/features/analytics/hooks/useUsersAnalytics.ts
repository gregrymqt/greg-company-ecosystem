import { useEffect, useState, useCallback } from "react";
import { socketService } from "@/shared/services/socket.service";
import { AppHubsBIFastAPI } from "@/shared/enums/hub.enums";
import { usersService } from "../services/users.service";
import { AlertService } from "@/shared/services/alert.service";
import type { UserSummary, UserWSPayload } from "../types/users.types";
import { ApiError } from "@/shared/services/api.service";

export const useUsersAnalytics = () => {
  const [summary, setSummary] = useState<UserSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const loadData = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await usersService.getSummary(); //
      setSummary(data);
    } catch (error) {
      if (error instanceof ApiError) {
        console.error(error.message);
        AlertService.error(
          "Erro em Usuários",
          "Não foi possível carregar as métricas de usuários.",
        );
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const hub = AppHubsBIFastAPI.BI_USERS;

    socketService.connect(hub);

    // Ouve o evento 'UserSummaryUpdate' disparado pelo Python
    socketService.on<UserWSPayload>(hub, "UserSummaryUpdate", (payload) => {
      setSummary(payload.summary);

      if (payload.type === "Manual") {
        AlertService.notify(
          "Usuários Atualizados",
          "Sincronização manual concluída.",
          "success",
        );
      } else {
        // Notifica apenas se houver novos usuários significativos (opcional)
        AlertService.notify(
          "Atualização de Base",
          "Novos usuários detectados no sistema.",
          "info",
        );
      }
    });

    loadData();

    return () => {
      socketService.off(hub, "UserSummaryUpdate");
    };
  }, [loadData]);

  const forceUpdate = async () => {
    try {
      // Invoca o handler 'ForceUpdate' no Python
      await socketService.invoke(AppHubsBIFastAPI.BI_USERS, "ForceUpdate", {});
      AlertService.notify(
        "Processando",
        "Recalculando taxas de conversão...",
        "info",
      );
    } catch (error) {
      if (error instanceof ApiError) {
        console.error(error.message);
        AlertService.error("Erro", error.message);
      }
    }
  };

  return { summary, isLoading, forceUpdate, refresh: loadData };
};
