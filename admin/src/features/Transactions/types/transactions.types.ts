// src/features/Transactions/shared/types/transactions.types.ts

export interface PaymentItems {
  id: string;
  amount: number;
  netReceivedAmount?: number;
  status: string;
  createdAt: string; 
  description?: string;
  paymentMethod?: string;
  payerEmail?: string;
  userId?: string;
}

export interface AdminPaymentPaginatedResponse {
  items: PaymentItems[];
  totalItems: number;
  totalPages: number;
  currentPage: number;
}

export interface RefundStatusData {
  status: 'pending' | 'completed' | 'failed';
  message?: string;
}

export interface TransactionFilters {
  status?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}
