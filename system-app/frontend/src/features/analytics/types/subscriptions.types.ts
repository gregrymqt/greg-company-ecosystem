export interface Subscription {
  Id: string;
  UserId: string;
  Status: string;
  CurrentAmount: number;
  CurrentPeriodEndDate: string;
  PlanName?: string;
}

export interface SubscriptionSummary {
  TotalSubscriptions: number;
  ActiveSubscriptions: number;
  MonthlyRecurringRevenue: number;
  ChurnRate: number;
}

export interface SubscriptionWSPayload {
  summary: SubscriptionSummary;
  type: "Manual" | "Auto";
}