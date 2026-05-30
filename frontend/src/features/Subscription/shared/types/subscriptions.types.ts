// ============================================
// ADMIN TYPES (Backend: AdminSubscriptionsController)
// ============================================

// Mapeia AutoRecurringDto do C#
export interface AdminAutoRecurring {
  frequency: number;
  frequency_type: string | null;
  transaction_amount: number;
  currency_id: string | null;
  start_date: string;
  end_date: string;
}

// Mapeia SubscriptionResponseDto do C# (Resultado da busca)
export interface AdminSubscriptionDetail {
  id: string;
  status: string;
  preapproval_plan_id: string | null;
  payer_id: number | null;
  payer_email: string | null;
  reason: string | null;
  date_created: string; // ISO Date
  last_modified: string; // ISO Date
  next_payment_date: string | null; // ISO Date
  auto_recurring: AdminAutoRecurring | null;
  payment_method_id: string | null;
}

// Payload para atualizar valor (UpdateSubscriptionValueDto)
export interface UpdateSubscriptionValuePayload {
  transaction_amount: number;
  currency_id: string; // Geralmente 'BRL'
}

// Payload para atualizar status (UpdateSubscriptionStatusDto)
export interface UpdateSubscriptionStatusPayload {
  status: 'authorized' | 'paused' | 'cancelled';
}

// ============================================
// PUBLIC TYPES (Backend: UserSubscriptionsController)
// ============================================

// Baseado no AutoRecurringDto
export interface AutoRecurringDto {
  frequency: number;
  frequency_type: string | null;
  transaction_amount: number;
  currency_id: string | null;
  start_date: string; // DateTime ISO string
  end_date: string;   // DateTime ISO string
}

// Baseado no SubscriptionResponseDto
export interface SubscriptionResponseDto {
  id: string | null;
  status: string | null;
  preapproval_plan_id: string | null;
  payer_id: number | null;
  payer_email: string | null;
  reason: string | null;
  date_created: string;
  last_modified: string;
  next_payment_date: string | null;
  auto_recurring: AutoRecurringDto | null;
  payment_method_id: string | null;
}

// Baseado no SubscriptionDetailsDto
export interface SubscriptionDetailsDto {
  subscriptionId: string | null;
  planName: string | null;
  status: string | null;
  amount: number;
  lastFourCardDigits: string | null;
  nextBillingDate: string | null;
}
