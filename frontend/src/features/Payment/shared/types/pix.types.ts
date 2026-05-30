// src/features/Payment/shared/types/pix.types.ts

export interface PixDocType {
  id: string;
  name: string;
}

// Alias para compatibilidade
export type IdentificationType = PixDocType;

export interface PixPayer {
  firstName: string;
  lastName: string;
  email: string;
  identificationType: string;
  identificationNumber: string;
}

// O que enviamos para o Backend
export interface CreatePixDTO {
  transactionAmount: number;
  description: string;
  paymentMethodId: string;
  planExternalId: string;
  payer: {
    firstName: string;
    lastName: string;
    email: string;
    identification: {
      type: string;
      number: string;
    };
  };
}

// O que o Backend retorna (Baseado no seu renderPaymentView)
export interface PixResponse {
  qrCode: string;       // O código "copia e cola"
  qrCodeBase64: string; // A imagem em base64
  paymentId: string;    // ID para monitorar via SignalR
  expirationDate?: string;
}

// Estados da UI do PIX
export type PixStep = 'FORM' | 'QR_CODE' | 'SUCCESS' | 'ERROR';
