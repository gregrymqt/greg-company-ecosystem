// src/pages/Payment/hooks/useCreditCardPayment.ts
import { useState } from "react";
import { CreditCardService } from "../services/creditCard.service";
import { AlertService } from "@/shared/services/alert.service";

import type { 
  BrickPaymentData, 
  CreditCardPaymentRequestDto, 
  CreditCardMode,
  PaymentSocketMessage
} from "../types";
import { useSocketListener } from "@/shared/hooks/useSocket";
import { ApiError } from "@/shared/services/api.service";
import { AppHubsCSharp } from "@/shared/enums/hub";

interface UseCreditCardProps {
  planId: string;   // Guid do plano
  frequency: number; // Para lógica "anual" (ex: 12) em vez de planName
  amount: number;
  mode: CreditCardMode;
  onSuccess: () => void;
}

export const useCreditCardPayment = ({ 
  planId, 
  frequency, 
  amount, 
  mode, 
  onSuccess 
}: UseCreditCardProps) => {
  
  // 1. Chave de idempotência estável para a sessão do checkout
  const [idempotencyKey] = useState(() => self.crypto.randomUUID());
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);



  // 2. Ouvinte de Mensagens do Backend (Processing, Approved, Failed)
  useSocketListener<PaymentSocketMessage>(
    AppHubsCSharp.GlobalRealtime,
    'UpdatePaymentStatus',
    (data) => {
      console.log('💳 Socket Update:', data);

      if (data.status === 'processing') {
        // Ex: "Validando seus dados..." ou "Comunicando com o provedor..."
        AlertService.notify('Processando', data.message, 'info');
      }

      if (data.status === 'approved') {
        AlertService.notify('Sucesso!', data.message, 'success');
        setLoading(false);
        onSuccess(); // Redireciona ou mostra tela final
      }

      if (data.status === 'failed' || data.status === 'error') {
        AlertService.error('Erro no Pagamento', data.message);
        setLoading(false);
        setError(data.message);
      }
    }
  );

  // 3. Função de Disparo (Chamada pelo Brick)
  const handleCreditCardSubmit = async (brickData: BrickPaymentData) => {
    setLoading(true);
    setError(null);

    try {
      // TRADUÇÃO DE DADOS: Frontend (Brick) -> Backend (DTO)
      // O Backend verifica: if (string.Equals(request.Plano, "anual"...))
      const isAnnual = frequency === 12;

      const payload: CreditCardPaymentRequestDto = {
        token: brickData.token,
        installments: brickData.installments,
        paymentMethodId: brickData.payment_method_id,
        issuerId: brickData.issuer_id,
        amount: amount,
        planExternalId: planId,
        // Envia "anual" para cair no CreateSubscriptionInternalAsync, senão CreateSinglePaymentInternalAsync
        plano: isAnnual ? 'anual' : 'mensal', 
        payer: {
          email: brickData.payer.email,
          // Brick as vezes não manda nome separado, ajustaremos se necessário
          identification: {
            type: brickData.payer.identification.type,
            number: brickData.payer.identification.number
          }
        }
      };

      // Dispara a requisição repassando a chave de idempotência preservada
      await CreditCardService.processPayment(payload, idempotencyKey);

      // Não setamos loading(false) aqui, esperamos o Socket dizer "approved" ou "failed"

    } catch (err) {
        if(err instanceof ApiError) {
          setError(err.message);
          setLoading(false);
        }
    }
  };

  return {
    loading,
    error,
    mode,
    handleCreditCardSubmit
  };
};
