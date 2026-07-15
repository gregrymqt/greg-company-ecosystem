
export interface PaymentItems {
  id: number;
  amount: number;
  status: string;
  createdAt: string; // O JSON retorna data como string ISO
  description?: string;
  paymentMethod?: string;
}

export interface RefundRequest {
  amount?: number;
}

export interface RefundStatusData {
  status: 'pending' | 'completed' | 'failed';
  message?: string;
}
