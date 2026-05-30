// src/features/Plan/Admin/services/adminPlans.service.ts
import { ApiService } from "@/shared/services/api.service";

import type { 
  PagedResult, 
  PlanAdminDetail, 
  PlanAdminSummary,
  CreatePlanRequest, 
  UpdatePlanRequest 
} from "../../shared";

const ADMIN_ENDPOINT = '/admin/plans';

/**
 * AdminPlansService - Consome AdminPlansController
 * Responsável por todas operações de gestão de planos (CRUD completo)
 */
export const AdminPlansService = {
  /**
   * C# Controller: AdminPlansController.GetPlans
   * Retorna a lista administrativa dos planos com paginação
   */
  getAdminPlans: async (page = 1, pageSize = 10): Promise<PagedResult<PlanAdminSummary>> => {
    const query = `?page=${page}&pageSize=${pageSize}`;
    return await ApiService.get<PagedResult<PlanAdminSummary>>(`${ADMIN_ENDPOINT}${query}`);
  },

  /**
   * C# Controller: AdminPlansController.GetPlanById
   * Retorna o DTO específico para preencher o formulário de edição
   */
  getById: async (id: string): Promise<PlanAdminDetail> => {
    return await ApiService.get<PlanAdminDetail>(`${ADMIN_ENDPOINT}/${id}`);
  },

  /**
   * C# Controller: AdminPlansController.CreatePlan
   * Cria um novo plano no Mercado Pago e sincroniza no banco local
   */
  create: async (data: CreatePlanRequest): Promise<PlanAdminSummary> => {
    return await ApiService.post<PlanAdminSummary>(`${ADMIN_ENDPOINT}`, data);
  },

  /**
   * C# Controller: AdminPlansController.UpdatePlan
   * Atualiza um plano existente (reason e/ou auto_recurring)
   */
  update: async (id: string, data: UpdatePlanRequest): Promise<unknown> => {
    return await ApiService.put(`${ADMIN_ENDPOINT}/${id}`, data);
  },

  /**
   * C# Controller: AdminPlansController.DeletePlan
   * Remove um plano do Mercado Pago e marca como inativo no banco
   */
  delete: async (id: string): Promise<void> => {
    return await ApiService.delete(`${ADMIN_ENDPOINT}/${id}`);
  }
};
