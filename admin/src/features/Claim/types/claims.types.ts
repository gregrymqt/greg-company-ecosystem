// src/types/claims.ts

import { type FieldValues } from "react-hook-form";

export const ClaimStatus = {
  Novo: 'Novo',
  EmAnalise: 'EmAnalise',
  ResolvidoGanhamos: 'ResolvidoGanhamos',
  ResolvidoPerdemos: 'ResolvidoPerdemos',
} as const;

export type ClaimStatus = typeof ClaimStatus[keyof typeof ClaimStatus];


// --- ENTIDADES VISUAIS (VIEW MODELS) ---

// Resumo para a lista (Tabela)
export interface ClaimSummary {
  internalId: string;   // ID do banco
  mpClaimId: string;    // ID do Mercado Pago
  customerName?: string;
  resourceId?: string;
  type: string;
  status: string;       // String vinda do backend ou Enum
  dateCreated: string;
  isUrgent?: boolean;   // Lógica visual
}

// Detalhe completo com Chat
export interface ClaimDetail {
  internalId: string;
  mpClaimId: string;
  status: string;
  stage?: string;
  resolution?: string;
  messages: ChatMessage[];
  canReply?: boolean;
}

// Mensagem individual do Chat
export interface ChatMessage {
  messageId: string;
  senderRole: string;   // 'complainant' | 'respondent' | 'mediator'
  content: string;
  dateCreated: string;
  attachments: string[]; // URLs
  isMe: boolean;        // Crucial para o layout (Direita/Esquerda)
}

// --- DTOs DE ENVIO (FORMULÁRIOS) ---

// Interface para o Form (Hook Form)
export interface ReplyFormData extends FieldValues {
  message: string;
  attachments?: FileList; // Input type="file" retorna FileList

}

// Payload JSON esperado pelo Backend (Admin)
export interface ReplyClaimPayload {
  internalId: string;
  message: string;
  // Nota: Backend atual espera JSON. Upload de arquivo requer endpoint separado ou FormData.
}

// Payload JSON esperado pelo Backend (User)
export interface ReplyUserPayload {
  message: string;
}
