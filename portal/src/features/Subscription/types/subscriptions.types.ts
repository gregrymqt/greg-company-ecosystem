// ============================================
// PUBLIC TYPES (Backend: UserSubscriptionsController)
// ============================================

export type SubscriptionStatusType = 'pending' | 'authorized' | 'paused' | 'cancelled' | 'unknown';

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
  status: SubscriptionStatusType | null;
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
  status: SubscriptionStatusType | null;
  amount: number;
  lastFourCardDigits: string | null;
  nextBillingDate: string | null;
}

export interface UpdateSubscriptionStatusDto {
  status: string;
}
