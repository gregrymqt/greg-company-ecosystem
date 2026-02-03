import { useEffect, useState, useCallback } from "react";
import { socketService } from "@/shared/services/socket.service";
import { AppHubsBIFastAPI } from "@/shared/enums/hub.enums";
import { storageService } from "../services/storage.service";
import { AlertService } from "@/shared/services/alert.service";
import type { StorageStats, StorageWSPayload } from "../types/storage.types";
import { ApiError } from "@/shared/services/api.service";

export const useStorageAnalytics = () => {
  const [stats, setStats] = useState<StorageStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const loadData = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await storageService.getStats();
      setStats(data);
    } catch (error) {
      if (error instanceof ApiError) {
        console.error(error.message);
        AlertService.error(`Erro ${error.status} de Storage`, error.message);
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const hub = AppHubsBIFastAPI.STORAGE;

    socketService.connect(hub);

    // Ouve o evento 'StorageStatsUpdate' definido no seu handler Python
    socketService.on<StorageWSPayload>(hub, "StorageStatsUpdate", (payload) => {
      setStats(payload.stats);

      if (payload.type === "Manual") {
        AlertService.notify(
          "Storage Atualizado",
          "Dados de uso de disco atualizados.",
          "success",
        );
      }
    });

    loadData();

    return () => {
      socketService.off(hub, "StorageStatsUpdate");
    };
  }, [loadData]);

  const forceUpdate = async () => {
    try {
      // Invoca o método no Python via WebSocket
      await socketService.invoke(AppHubsBIFastAPI.STORAGE, "ForceUpdate", {});
      AlertService.notify(
        "Solicitado",
        "Recalculando espaço em disco...",
        "info",
      );
    } catch (error) {
      if (error instanceof ApiError) {
        console.error(error.message);
        AlertService.error(`Erro ${error.status} de Storage`, error.message);
      }
    }
  };

  return { stats, isLoading, forceUpdate, refresh: loadData };
};
