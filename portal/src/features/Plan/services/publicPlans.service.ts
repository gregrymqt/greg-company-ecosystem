// src/features/Plan/Public/services/publicPlans.service.ts
import { ApiService } from "@/shared/services/api.service";
import type { PagedResult, PlanPublic } from "../types/plan.types";



const PUBLIC_ENDPOINT = '/public/plans';

/**
 * PublicPlansService - Consome PublicPlansController
 * Responsável apenas por leitura de planos ativos para área pública (vitrine)
 */
export const PublicPlansService = {
  /**
   * C# Controller: PublicPlansController.GetPlans
   * Retorna planos ativos formatados para exibição pública
   */
  getPublicPlans: async (page = 1, pageSize = 10): Promise<PagedResult<PlanPublic>> => {
    const query = `?page=${page}&pageSize=${pageSize}`;
    return await ApiService.get<PagedResult<PlanPublic>>(`${PUBLIC_ENDPOINT}${query}`);
  },

  /**
   * C# Controller: PublicPlansController.GetById
   * Retorna os detalhes de um plano específico pelo ID.
   * (Opcional: pode ser usado pelo hook de checkout se preferir buscar o plano separadamente)
   */
  getPlanById: async (planId: string): Promise<PlanPublic> => {
    return await ApiService.get<PlanPublic>(`${PUBLIC_ENDPOINT}/${planId}`);
  }
};
