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
} from '../types/support.types';

class AdminSupportService {
  private readonly BASE_PATH = '/admin/support/tickets';

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
   * PUT /admin/support/tickets/{id}/status
   */
  async updateTicketStatus(
    id: string,
    status: SupportTicketStatus
  ): Promise<SupportApiResponse<void>> {
    const payload: UpdateSupportTicketDto = { status };
    return await ApiService.put<SupportApiResponse<void>>(
      `${this.BASE_PATH}/${id}/status`,
      payload
    );
  }

  /**
   * Atribui ticket a um admin
   * PUT /admin/support/tickets/{id}/assign
   */
  async assignTicket(id: string): Promise<SupportApiResponse<void>> {
    return await ApiService.put<SupportApiResponse<void>>(
      `${this.BASE_PATH}/${id}/assign`,
      {}
    );
  }

  /**
   * Responde a um ticket
   * POST /admin/support/tickets/{id}/reply
   */
  async replyToTicket(id: string, message: string): Promise<SupportApiResponse<void>> {
    return await ApiService.post<SupportApiResponse<void>>(
      `${this.BASE_PATH}/${id}/reply`,
      { message }
    );
  }
}

export const adminSupportService = new AdminSupportService();
