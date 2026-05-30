/**
 * User Account Service
 * Alinhado com UserAccountController.cs
 * Endpoints: /user-account/*
 */

import { ApiService } from '@/shared/services/api.service';
import type { UserProfileDto, AvatarUpdateResponse } from '../../shared';

class UserAccountService {
  private readonly BASE_PATH = '/user-account';

  /**
   * Busca o perfil do usuário autenticado
   * GET /user-account/profile
   */
  async getProfile(): Promise<UserProfileDto> {
    return await ApiService.get<UserProfileDto>(`${this.BASE_PATH}/profile`);
  }

  /**
   * Atualiza o avatar do usuário
   * POST /user-account/avatar
   */
  async updateAvatar(file: File): Promise<AvatarUpdateResponse> {
    return await ApiService.postWithFile<AvatarUpdateResponse, Record<string, never>>(
      `${this.BASE_PATH}/avatar`,
      {},
      file,
      'avatar',
      undefined,
      true // bypass smart logic - é só 1 arquivo de imagem
    );
  }
}

export const userAccountService = new UserAccountService();
