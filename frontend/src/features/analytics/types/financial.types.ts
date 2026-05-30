export interface PaymentSummary {
  TotalPayments: number;
  TotalApproved: number;
  TotalPending: number;
  TotalCancelled: number;
  UniqueCustomers: number;
  AvgTicket: number;
  ApprovalRate: number;
}

export interface RevenueMetrics {
  TotalRevenue: number;
  MonthlyRevenue: number;
  YearlyRevenue: number;
  TotalTransactions: number;
  AverageTransactionValue: number;
  TopPaymentMethod: string;
  PaymentMethodDistribution: Record<string, number>;
}

export interface ChargebackSummary {
  TotalChargebacks: number;
  TotalAmount: number;
  Novo: number;
  AguardandoEvidencias: number;
  Ganhamos: number;
  Perdemos: number;
  WinRate: number;
}