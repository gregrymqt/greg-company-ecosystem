import { ApiService } from "@/shared/services/api.service";

import type { 
  PagedResult, 
  PlanPublic, 
  PlanAdminDetail, 
  PlanAdminSummary,
  CreatePlanRequest, 
  UpdatePlanRequest 
} from "@/features/Plan/types/plans.type";

const ADMIN_ENDPOINT = '/admin/plans';
const PUBLIC_ENDPOINT = '/public/plans';

export const PlanService = {
  // ============================================================
  // ÁREA PÚBLICA (ALLOW)
  // ============================================================

  // C# Controller: PublicPlansController.GetPlans
  getPublicPlans: async (page = 1, pageSize = 10): Promise<PagedResult<PlanPublic>> => {
    const query = `?page=${page}&pageSize=${pageSize}`;
    return await ApiService.get<PagedResult<PlanPublic>>(`${PUBLIC_ENDPOINT}${query}`);
  },

  // ============================================================
  // ÁREA ADMINISTRATIVA (ADMIN)
  // ============================================================

  // C# Controller: AdminPlansController.GetPlans
  // Retorna a lista crua/administrativa dos planos
  getAdminPlans: async (page = 1, pageSize = 10): Promise<PagedResult<PlanAdminSummary>> => {
    const query = `?page=${page}&pageSize=${pageSize}`;
    return await ApiService.get<PagedResult<PlanAdminSummary>>(`${ADMIN_ENDPOINT}${query}`);
  },

  // C# Controller: AdminPlansController.GetPlanById
  // Retorna o DTO específico para preencher o formulário de edição
  getById: async (id: string): Promise<PlanAdminDetail> => {
    return await ApiService.get<PlanAdminDetail>(`${ADMIN_ENDPOINT}/${id}`);
  },

  // C# Controller: AdminPlansController.CreatePlan
  create: async (data: CreatePlanRequest): Promise<PlanAdminSummary> => {
    return await ApiService.post<PlanAdminSummary>(`${ADMIN_ENDPOINT}`, data);
  },

  // C# Controller: AdminPlansController.UpdatePlan
  update: async (id: string, data: UpdatePlanRequest): Promise<unknown> => {
    return await ApiService.put(`${ADMIN_ENDPOINT}/${id}`, data);
  },

  // C# Controller: AdminPlansController.DeletePlan
  delete: async (id: string): Promise<void> => {
    return await ApiService.delete(`${ADMIN_ENDPOINT}/${id}`);
  }
};