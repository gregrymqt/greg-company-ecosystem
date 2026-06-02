/**
 * Tipagem compartilhada de Support - Alinhada com DTOs do Backend
 * 
 * Esta é a fonte única de verdade (Single Source of Truth) para tipos de Support.
 * Tanto Admin quanto Public devem importar daqui.
 */

// =================================================================
// ENUMS E STATUS
// =================================================================

export type SupportTicketStatus = 'Open' | 'InProgress' | 'Closed';

// =================================================================
// DTOs - Baseados em SupportDTO.cs
// =================================================================

/**
 * Ticket de suporte (response do backend)
 * Baseado em SupportTicketResponseDto.cs
 */
export interface SupportTicketDto {
  id: string;
  userId: string;
  context: string;
  explanation: string;
  status: SupportTicketStatus;
  createdAt: string; // ISO 8601 date string
}

/**
 * Payload para criar novo ticket
 * Baseado em CreateSupportTicketDto.cs
 */
export interface CreateSupportTicketDto {
  context: string;
  explanation: string;
}

/**
 * Payload para atualizar status (Admin only)
 * Baseado em UpdateSupportTicketDto.cs
 */
export interface UpdateSupportTicketDto {
  status: SupportTicketStatus;
}

// =================================================================
// API RESPONSES
// =================================================================

/**
 * Resposta genérica da API de Support
 */
export interface SupportApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
}

/**
 * Resultado paginado de tickets
 */
export interface PaginatedSupportResult {
  items: SupportTicketDto[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Filtros para listagem de tickets (Admin)
 */
export interface SupportFilters {
  page: number;
  pageSize: number;
  status?: SupportTicketStatus;
  searchTerm?: string;
}
