// src/features/Plan/types/plan.types.ts

// --- GENERIC SHARED ---

export interface PagedResult<T> {
  items: T[];
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// --- PUBLIC (ALLOW) TYPES ---
// Baseado no C# PlanDto
// Usado na tela de preços/assinatura pública
export interface PlanPublic {
  publicId: string;
  name: string;
  slug: string;
  amount: number; // Necessário para os payloads e UI do front
  priceDisplay: string;
  billingInfo: string;
  features: string[]; // Lista de benefícios
  isRecommended: boolean;
  isActive: boolean;
  frequency: number; // 1 = mensal, 12 = anual, etc.
  category: 'course' | 'ecommerce';
}

