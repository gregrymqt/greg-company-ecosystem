import type { PlanPublic } from '@/features/Plan/types/plan.types';

export interface PaymentLayoutProps {
  plan?: PlanPublic; // O plano pode ser lido dinamicamente agora via URL/hooks
  userParams?: {
    email: string;
    name: string;
  };
}

// Identificadores das abas da Sidebar
export type PaymentMethodId = 'pix' | 'credit-card';

export interface PaymentSocketMessage {
  message: string;      // "Pagamento processado com sucesso!", "A processar..."
  status: 'processing' | 'approved' | 'failed' | 'error';
  isCompleted: boolean; // Indica se o fluxo terminou
  paymentId?: number;
}
