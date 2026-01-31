/**
 * Admin Support Service
 * Gerenciamento completo de tickets de suporte
 * Alinhado com SupportController.cs
 */

import { ApiService } from '@/shared/services/api.service';
import type {
  SupportTicketDto,
  SupportTicketStatus,
  UpdateSupportTicketDto,
  SupportApiResponse,
  PaginatedSupportResult,
  SupportFilters
} from '../../shared';

class AdminSupportService {
  private readonly BASE_PATH = '/support';

  /**
   * Lista todos os tickets com paginação
   * GET /support?page=X&pageSize=Y
   */
  async getAllTickets(filters: SupportFilters): Promise<SupportApiResponse<PaginatedSupportResult>> {
    const params = new URLSearchParams({
      page: filters.page.toString(),
      pageSize: filters.pageSize.toString()
    });

    if (filters.status) {
      params.append('status', filters.status);
    }

    if (filters.searchTerm) {
      params.append('searchTerm', filters.searchTerm);
    }

    return await ApiService.get<SupportApiResponse<PaginatedSupportResult>>(
      `${this.BASE_PATH}?${params.toString()}`
    );
  }

  /**
   * Busca ticket por ID
   * GET /support/{id}
   */
  async getTicketById(id: string): Promise<SupportApiResponse<SupportTicketDto>> {
    return await ApiService.get<SupportApiResponse<SupportTicketDto>>(
      `${this.BASE_PATH}/${id}`
    );
  }

  /**
   * Atualiza status do ticket
   * PUT /support/{id}
   */
  async updateTicketStatus(
    id: string,
    status: SupportTicketStatus
  ): Promise<SupportApiResponse<void>> {
    const payload: UpdateSupportTicketDto = { status };
    return await ApiService.put<SupportApiResponse<void>>(
      `${this.BASE_PATH}/${id}`,
      payload
    );
  }
}

export const adminSupportService = new AdminSupportService();
