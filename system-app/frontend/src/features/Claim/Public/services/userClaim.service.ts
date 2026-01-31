import { ApiService } from "@/shared/services/api.service";
import type { ClaimSummary, ClaimDetail } from '../../shared';

/**
 * User Claim Service - Consume UserClaimsController
 * Endpoints: /user/claims/*
 */
export const UserClaimService = {
  /**
   * Lista de reclamações do usuário logado
   */
  getMyClaims: async () => {
    return await ApiService.get<ClaimSummary[]>("/user/claims");
  },

  /**
   * Detalhes de uma claim específica do usuário (com chat)
   */
  getMyDetails: async (internalId: number) => {
    return await ApiService.get<ClaimDetail>(`/user/claims/${internalId}`);
  },

  /**
   * Enviar resposta como Usuário
   */
  reply: async (internalId: number, message: string) => {
    const payload = { message };
    return await ApiService.post(`/user/claims/${internalId}/reply`, payload);
  },

  /**
   * Solicitar mediação do Mercado Pago (escalar a disputa)
   */
  requestMediation: async (internalId: number) => {
    return await ApiService.post(`/user/claims/${internalId}/mediation`, {});
  },
};
