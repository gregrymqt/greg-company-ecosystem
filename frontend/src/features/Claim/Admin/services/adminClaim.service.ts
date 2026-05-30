import { ApiService } from "@/shared/services/api.service";
import type {
  ClaimSummary,
  ClaimDetail,
  ReplyClaimPayload,
} from '../../shared';

/**
 * Admin Claim Service - Consume AdminClaimsController
 * Endpoints: /admin/claims/*
 */
export const AdminClaimService = {
  /**
   * Lista paginada de todas as claims (Inbox do Administrador)
   */
  getAll: async (page = 1, searchTerm = "", statusFilter = "") => {
    const query = `?page=${page}&searchTerm=${searchTerm}&statusFilter=${statusFilter}`;
    return await ApiService.get<{
      claims: ClaimSummary[];
      totalPages: number;
    }>(`/admin/claims${query}`);
  },

  /**
   * Detalhes completos com Chat de uma claim específica
   */
  getDetails: async (id: number) => {
    return await ApiService.get<ClaimDetail>(`/admin/claims/${id}`);
  },

  /**
   * Enviar resposta ao usuário como Administrador
   */
  reply: async (internalId: number, message: string) => {
    const payload: ReplyClaimPayload = { internalId, message };
    return await ApiService.post("/admin/claims/reply", payload);
  },
};
