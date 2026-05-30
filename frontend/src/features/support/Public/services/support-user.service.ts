/**
 * User Support Service (Public Area)
 * Permite usuários criarem tickets
 * Alinhado com SupportController.cs
 */

import { ApiService } from '@/shared/services/api.service';
import type {
  CreateSupportTicketDto,
  SupportApiResponse
} from '../../shared';

class UserSupportService {
  private readonly BASE_PATH = '/support';

  /**
   * Cria um novo ticket de suporte
   * POST /support
   */
  async createTicket(payload: CreateSupportTicketDto): Promise<SupportApiResponse<void>> {
    return await ApiService.post<SupportApiResponse<void>>(
      this.BASE_PATH,
      payload
    );
  }
}

export const userSupportService = new UserSupportService();
