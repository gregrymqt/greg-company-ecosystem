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
  priceDisplay: string;
  billingInfo: string;
  features: string[]; // Lista de benefícios
  isRecommended: boolean;
  isActive: boolean;
  frequency: number; // 1 = mensal, 12 = anual, etc.
}

// --- ADMIN TYPES ---

// Baseado no C# PlanEditDto
// Usado no formulário de edição (GET by ID)
export interface PlanAdminDetail {
  publicId: string;
  name: string;
  transactionAmount: number;
  frequency: number;
  frequencyType: string;
  description: string;
}

// Baseado no C# PlanResponseDto
// Usado na listagem administrativa (Grid do Admin)
export interface PlanAdminSummary {
  id: string;      // MP ID
  reason: string;  // Nome do plano no MP
  status: string;  // active, etc.
  date_created: string;
  external_reference?: string;
  auto_recurring?: AutoRecurringRequest;
}

// --- PAYLOADS (REQUESTS) ---
// Atenção: Propriedades em snake_case para bater com o [JsonPropertyName] do C#

// Baseado no C# AutoRecurringDto
export interface AutoRecurringRequest {
  frequency: number;
  frequency_type: string;
  transaction_amount: number;
  currency_id: string;
}

// Baseado no C# CreatePlanDto
export interface CreatePlanRequest {
  reason: string;               // Nome do plano
  description: string;          // Descrição interna
  auto_recurring: AutoRecurringRequest;
}

// Baseado no C# UpdatePlanDto
export interface UpdatePlanRequest {
  reason?: string;
  auto_recurring?: AutoRecurringRequest;
}