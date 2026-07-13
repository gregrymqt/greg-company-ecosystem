import { useState, useEffect } from "react";
import { socketService } from "@/shared/services/socket.service"; // Ajuste o path conforme seu projeto
import { AppHubsCSharp } from "@/shared/enums/hub/hub.enums";     // Ajuste o path conforme seu projeto
import { useSocketListener } from "@/shared/hooks/useSocket";

interface VideoProgressPayload {
  percentage: number;
  status: "PROCESSING" | "SUCCESS" | "FAILED";
  message?: string;
}

export function useVideoProgress(storageIdentifier: string | null) {
  const [progress, setProgress] = useState<number>(0);
  const [status, setStatus] = useState<string>("IDLE");
  const [error, setError] = useState<string | null>(null);

  // 1. Escuta o evento vindo do Backend (Tratado automaticamente pelo seu useSocketListener)
  useSocketListener(
    AppHubsCSharp.GlobalRealtime,
    "ReceiveVideoProgress",
    (data: VideoProgressPayload) => {
      setProgress(data.percentage);
      setStatus(data.status);
      if (data.status === "FAILED") {
        setError(data.message || "Erro ao processar o vídeo.");
      }
    }
  );

  // 2. Solicita a inscrição no grupo de progresso do SignalR assim que o ID do vídeo muda
  useEffect(() => {
    if (!storageIdentifier) return;

    const joinVideoGroup = async () => {
      try {
        setError(null);
        // Invoca o método do backend para colocar a conexão no grupo "processing-{id}"
        await socketService.invoke(
          AppHubsCSharp.GlobalRealtime,
          "SubscribeToJobProgress",
          storageIdentifier
        );
        setStatus("PROCESSING");
      } catch (err) {
        console.error("Erro ao se inscrever no grupo de realtime do vídeo:", err);
        setError("Não foi possível conectar ao rastreamento do vídeo.");
      }
    };

    joinVideoGroup();
  }, [storageIdentifier]);

  return { progress, status, error };
}