// src/features/Transactions/shared/types/transactions.types.ts

export interface PaymentItems {
  id: string;
  amount: number;
  status: string;
  createdAt: string; // O JSON retorna data como string ISO
  description?: string;
  paymentMethod?: string;
}

export interface RefundStatusData {
  status: 'pending' | 'completed' | 'failed';
  message?: string;
}
