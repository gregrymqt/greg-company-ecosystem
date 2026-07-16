/**
 * Tipagem compartilhada de Support - Alinhada com DTOs do Backend
 * 
 * Esta é a fonte única de verdade (Single Source of Truth) para tipos de Support.
 * Tanto Admin quanto Public devem importar daqui.
 */

// =================================================================
// ENUMS E STATUS
// =================================================================

export type SupportTicketStatus = 'open' | 'in_progress' | 'resolved' | 'closed';
export type SupportTicketCategory = 'finance' | 'technical' | 'academic' | 'other';
export type SupportTicketPriority = 'low' | 'medium' | 'high';

// =================================================================
// DTOs - Baseados em SupportDTO.cs
// =================================================================

export interface SupportResponseDto {
  senderId: string;
  senderRole: string;
  message: string;
  dateCreated: string;
}

/**
 * Ticket de suporte (response do backend)
 * Baseado em SupportTicketResponseDto.cs
 */
export interface SupportTicketDto {
  id: string;
  userId: string;
  title: string;
  category: SupportTicketCategory;
  priority: SupportTicketPriority;
  status: SupportTicketStatus;
  assignedTo?: string;
  responses: SupportResponseDto[];
  createdAt: string; // ISO 8601 date string
  lastUpdated: string;
}

/**
 * Payload para criar novo ticket
 * Baseado em CreateSupportTicketDto.cs
 */
export interface CreateSupportTicketDto {
  title: string;
  category: SupportTicketCategory;
  priority: SupportTicketPriority;
  message: string;
}

/**
 * Payload para atualizar status (Admin only)
 * Baseado em UpdateSupportTicketDto.cs
 */
export interface UpdateSupportTicketDto {
  status?: SupportTicketStatus;
  priority?: SupportTicketPriority;
}

export interface ReplyToTicketDto {
  message: string;
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
  priority?: SupportTicketPriority;
  searchTerm?: string;
}
