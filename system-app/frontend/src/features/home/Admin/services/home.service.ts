/**
 * Admin Home Service - Endpoints para área administrativa
 * Operações CRUD completas (Create, Read, Update, Delete)
 */

import { ApiService } from '@/shared/services/api.service';
import type {
  HeroSlideDto,
  ServiceDto,
  HeroFormValues,
  ServiceFormValues
} from '@/features/home/shared/types/home.types';

const BASE_ENDPOINT = '/Home';

export const adminHomeService = {
  // =================================================================
  // HERO SLIDES (Com Upload de Imagem)
  // =================================================================

  /**
   * Cria novo Hero Slide com upload de imagem
   * POST: /api/Home/hero
   */
  createHero: async (data: HeroFormValues): Promise<HeroSlideDto> => {
    const { newImage, ...dto } = data;
    const file = newImage && newImage.length > 0 ? newImage[0] : null;

    return await ApiService.postWithFile<HeroSlideDto, typeof dto>(
      `${BASE_ENDPOINT}/hero`,
      dto,
      file,
      'file'
    );
  },

  /**
   * Atualiza Hero Slide existente (com possível nova imagem)
   * PUT: /api/Home/hero/{id}
   */
  updateHero: async (id: number, data: HeroFormValues): Promise<void> => {
    const { newImage, ...dto } = data;
    const file = newImage && newImage.length > 0 ? newImage[0] : null;

    return await ApiService.putWithFile<void, typeof dto>(
      `${BASE_ENDPOINT}/hero/${id}`,
      dto,
      file,
      'file'
    );
  },

  /**
   * Remove Hero Slide
   * DELETE: /api/Home/hero/{id}
   */
  deleteHero: async (id: number): Promise<void> => {
    return await ApiService.delete(`${BASE_ENDPOINT}/hero/${id}`);
  },

  // =================================================================
  // SERVICES (JSON Padrão - Sem Upload de Arquivo)
  // =================================================================

  /**
   * Cria novo Service
   * POST: /api/Home/services
   */
  createService: async (data: ServiceFormValues): Promise<ServiceDto> => {
    return await ApiService.post<ServiceDto>(`${BASE_ENDPOINT}/services`, data);
  },

  /**
   * Atualiza Service existente
   * PUT: /api/Home/services/{id}
   */
  updateService: async (id: number, data: ServiceFormValues): Promise<void> => {
    return await ApiService.put(`${BASE_ENDPOINT}/services/${id}`, data);
  },

  /**
   * Remove Service
   * DELETE: /api/Home/services/{id}
   */
  deleteService: async (id: number): Promise<void> => {
    return await ApiService.delete(`${BASE_ENDPOINT}/services/${id}`);
  }
};
