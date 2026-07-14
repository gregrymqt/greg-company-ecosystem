// src/types/claims.ts

import { type FieldValues } from "react-hook-form";


// --- ENTIDADES VISUAIS (VIEW MODELS) ---

// Resumo para a lista (Tabela)
export interface ClaimSummary {
  internalId: string;   // ID do banco
  mpClaimId: number;    // ID do Mercado Pago
  customerName?: string;
  customerEmail?: string;
  resourceId?: string;
  type: string;
  status: string;       // String vinda do backend ou Enum
  dateCreated: string;
  isUrgent: boolean;    // Lógica visual
}

// Detalhe completo com Chat
export interface ClaimDetail {
  internalId: string;
  mpClaimId: number;
  status: string;
  stage?: string;
  playerRole?: string;
  messages: ChatMessage[];
  canReply: boolean;
}

// Mensagem individual do Chat
export interface ChatMessage {
  messageId: string;
  senderRole: string;   // 'complainant' | 'respondent' | 'mediator'
  content: string;
  dateCreated: string;
  attachments: string[]; // URLs
  isMe: boolean;        // Crucial para o layout (Direita/Esquerda)
  isMediator: boolean;
}

// --- DTOs DE ENVIO (FORMULÁRIOS) ---

// Interface para o GenericForm (Hook Form)
export interface ReplyFormData extends FieldValues {
  message: string;
  attachments?: FileList; // GenericForm retorna FileList
}

// Payload JSON esperado pelo Backend (User)
export interface ReplyUserPayload {
  message: string;
}
