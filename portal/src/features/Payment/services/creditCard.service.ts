// src/pages/Payment/services/credit-card.service.ts
import { ApiService } from "@/shared/services/api.service";
import type { CreditCardPaymentRequestDto, PaymentResponse } from "../types";

export const CreditCardService = {
  processPayment: async (data: CreditCardPaymentRequestDto, idempotencyKey: string): Promise<PaymentResponse> => {
    // A chave de idempotência agora é recebida por parâmetro da camada superior
    // O ApiService já concatena '/api', então chamamos '/credit/card/process-payment'
    return await ApiService.post<PaymentResponse>(
      '/credit/card/process-payment',
      data,
      {
        headers: {
          'X-Idempotency-Key': idempotencyKey
        }
      }
    );
  }
};
