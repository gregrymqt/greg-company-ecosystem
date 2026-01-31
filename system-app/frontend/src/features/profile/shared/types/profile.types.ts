/**
 * Tipagem compartilhada de Profile - Alinhada com DTOs do Backend
 * 
 * Esta é a fonte única de verdade (Single Source of Truth) para tipos de Profile.
 * Tanto Admin quanto Account devem importar daqui.
 */

// =================================================================
// USER ACCOUNT - Baseado em UserProfileDto.cs
// =================================================================

/**
 * Perfil do usuário autenticado
 * Baseado em UserProfileDto.cs
 */
export interface UserProfileDto {
  name: string;
  email: string;
  avatarUrl: string;
}

/**
 * Resposta do upload de avatar
 * Baseado em AvatarUpdateResponse.cs
 */
export interface AvatarUpdateResponse {
  avatarUrl: string;
  message: string;
}

// =================================================================
// ADMIN STUDENTS - Baseado em StudentDto.cs
// =================================================================

/**
 * Dados de um estudante para visualização administrativa
 * Baseado em StudentDto (record) no backend
 */
export interface StudentDto {
  id: string;
  name: string;
  email: string;
  subscriptionStatus?: string;  // "active", "cancelled", "paused"
  planName?: string;             // Nome do plano assinado
  registrationDate: string;      // ISO 8601 date string
  subscriptionId?: string;
}

/**
 * Resultado paginado genérico
 * Baseado em PaginatedResult<T>.cs
 */
export interface PaginatedResult<T> {
  items: T[];
  currentPage: number;
  totalPages: number;
  totalCount: number;
}

/**
 * Filtros para listagem de estudantes
 */
export interface StudentFilters {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  subscriptionStatus?: string;
}
