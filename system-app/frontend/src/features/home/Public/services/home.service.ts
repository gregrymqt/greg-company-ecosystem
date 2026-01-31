/**
 * Public Home Service - Endpoints para área pública
 * Somente operações de leitura (GET)
 */

import { ApiService } from '@/shared/services/api.service';
import type { HomeContentDto } from '@/features/home/shared/types/home.types';

const BASE_ENDPOINT = '/Home';

export const publicHomeService = {
  /**
   * Busca todo o conteúdo da Home (Hero + Services)
   * GET: /api/Home
   */
  getHomeContent: async (): Promise<HomeContentDto> => {
    return await ApiService.get<HomeContentDto>(BASE_ENDPOINT);
  }
};
