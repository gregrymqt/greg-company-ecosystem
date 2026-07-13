import { useState, useEffect, useCallback } from "react";
import { UserClaimService } from '../services/userClaim.service';
import type { ChatMessage } from '../types/claims.types';
import type { ReplyFormData } from '../types/claim.dtos';
import { AlertService } from "@/shared/services/alert.service";
import { ApiError } from "@/shared/services/api.service";
import { useSocketListener } from "@/shared/hooks/useSocket";
import { AppHubsCSharp } from "@/shared/enums/hub/hub.enums";

// Props para saber quem está usando o hook
interface UseClaimChatProps {
  claimId: number;
}

export const useClaimChatLogic = ({ claimId }: UseClaimChatProps) => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isSending, setIsSending] = useState(false);

  // 1. Função de Busca (Memoizada para usar no useEffect)
  const fetchMessages = useCallback(async (silent = false) => {
    try {
      if (!silent) setIsLoading(true);
      
      const data = await UserClaimService.getMyDetails(claimId);

      if (data && data.messages) {
        setMessages(data.messages);
      }
    } catch (error) {
      console.error("Erro ao buscar mensagens", error);
    } finally {
      if (!silent) setIsLoading(false);
    }
  }, [claimId]);

  // 2. Carga Inicial
  useEffect(() => {
    fetchMessages(); // Carga inicial
  }, [fetchMessages]);

  // 3. Listener do WebSockets (Substituindo o Polling)
  useSocketListener(AppHubsCSharp.GlobalRealtime, "ReceiveMessage", () => {
    // Atualização silenciosa quando chega mensagem via SignalR
    fetchMessages(true);
  });

  // 3. Função de Envio (Conectada ao GenericForm)
  const handleSendResponse = async (formData: ReplyFormData) => {
    // Validação básica
    if (!formData.message) return;

    setIsSending(true);
    try {
      await UserClaimService.reply(claimId, formData.message);

      // Feedback visual e atualização
      AlertService.notify("Sucesso", "Mensagem enviada.", "success");

      // Limpa o form (o GenericForm cuida do reset se configurado, ou atualizamos a lista)
      await fetchMessages();
      return true; // Retorna true para o GenericForm saber que deu certo
    } catch (error) {
      if (error instanceof ApiError) {
        AlertService.error("Erro", error.message);
        return false;
      }
      AlertService.error("Erro", "Não foi possível enviar a mensagem.");
      return false;
    } finally {
      setIsSending(false);
    }
  };

  // 4. Função Extra para User: Escalar Mediação
  const handleRequestMediation = async () => {
    const { isConfirmed } = await AlertService.confirm(
      "Tem certeza?",
      "Isso envolverá o Mercado Pago para julgar o caso.",
    );

    if (isConfirmed) {
      try {
        await UserClaimService.requestMediation(claimId);
        AlertService.success("Solicitada", "O Mercado Pago irá intervir.");
        await fetchMessages(); // Atualiza status
      } catch (error) {
        if (error instanceof ApiError) {
          AlertService.error("Erro", error.message);
          return;
        }
        AlertService.error("Erro", "Falha ao solicitar mediação.");
      }
    }
  };

  return {
    messages,
    handleSendResponse,
    handleRequestMediation, // Exportado caso precise usar no botão
    isLoading: isLoading && messages.length === 0, // Loading inicial
    isSending,
  };
};
