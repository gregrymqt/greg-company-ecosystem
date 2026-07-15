import { useState, useEffect, useCallback } from "react";

import { AdminClaimService } from '../services/adminClaim.service';
import type { ChatMessage } from '../types/claims.types';
import type { ReplyFormData } from '../types/claim.dtos';
import { AlertService } from "@/shared/services/alert.service";
import { ApiError } from "@/shared/services/api.service";

// Socket Hooks
import { useSocketListener } from "@/shared/hooks/useSocket";
import { AppHubsCSharp } from "@/shared/enums/hub/hub.enums";

// Props para saber quem está usando o hook
interface UseClaimChatProps {
  claimId: string;
}

export const useClaimChatLogic = ({ claimId }: UseClaimChatProps) => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isSending, setIsSending] = useState(false);

  // 1. Função de Busca (Memoizada para usar no useEffect)
  const fetchMessages = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await AdminClaimService.getDetails(claimId);

      if (data && data.messages) {
        setMessages(data.messages);
      }
    } catch (error) {
      console.error("Erro ao buscar mensagens", error);
    } finally {
      setIsLoading(false);
    }
  }, [claimId]);

  // Carga inicial
  useEffect(() => {
    fetchMessages();
  }, [fetchMessages]);

  // 2. Ouvinte de Socket em Tempo Real
  // O backend deve enviar um payload contendo o claimId e o objeto da nova mensagem
  useSocketListener<{ claimId: string; message: ChatMessage }>(
    AppHubsCSharp.GlobalRealtime,
    "ReceiveClaimMessage",
    (payload) => {
      // Se a mensagem for deste claim específico, adiciona à lista
      if (payload.claimId === claimId) {
        setMessages((prev) => [...prev, payload.message]);
      }
    }
  );

  // 3. Função de Envio (Conectada ao GenericForm)
  const handleSendResponse = async (formData: ReplyFormData) => {
    // Validação básica
    if (!formData.message) return;

    if (formData.attachments && formData.attachments.length > 0) {
      AlertService.error(
        "Aviso",
        "O envio de arquivos anexos via painel de mediação está temporariamente indisponível. Remova os arquivos e envie apenas texto."
      );
      return false;
    }

    setIsSending(true);
    try {
      await AdminClaimService.reply(claimId, formData.message);

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
  return {
    messages,
    handleSendResponse,
    
    isLoading: isLoading && messages.length === 0, // Loading inicial
    isSending,
  };
};
