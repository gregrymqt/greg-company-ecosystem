import { ApiService } from "@/shared/services/api.service";

export const PreferenceService = {
  /**
   * Cria uma preferência de pagamento para um plano específico.
   * @param planId - O ID do plano a ser adquirido.
   * @param idempotencyKey - A chave de idempotência para garantir operações únicas.
   * @returns O ID da preferência gerada.
   */
  createPreference: async (planId: string, idempotencyKey: string): Promise<string> => {
    // Agora enviamos apenas o planId, o backend preenche os valores monetários com base nele
    const payload = { 
      planId
    };
    
    const response = await ApiService.post<{ preferenceId: string }>(
      '/preferences', 
      payload,
      {
        headers: {
          'X-Idempotency-Key': idempotencyKey
        }
      }
    );
    return response.preferenceId;
  }
};
