// Reflete: ClaimAnalyticsDTO
export interface ClaimAnalytics {
  id: number;
  mp_claim_id: number;
  amount_at_risk: number;
  claim_type: string;
  internal_status: string;
  days_open: number;
  panel_url: string;
  is_critical: boolean; // Calculado pela @property no Python
  user_name?: string;
  user_email?: string;
}

// Reflete: ClaimSummaryDTO
export interface ClaimSummary {
  TotalClaims: number;
  OpenedClaims: number;
  TotalAmountAtRisk: number;
  DisputeRate: number;
  WinRate: number;
  AverageResolutionDays?: number;
}

// Reflete: ClaimByReasonDTO
export interface ClaimByReason {
  Reason: string;
  Count: number;
  TotalAmount: number;
  Percentage: number;
}