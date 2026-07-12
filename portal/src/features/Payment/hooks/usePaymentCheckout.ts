import { useState, useEffect } from 'react';
import type { PlanPublic } from '@/features/Plan/types/plan.types';
import { PreferenceService } from '../services/preference.service';
import { PublicPlansService } from '@/features/Plan';

export interface UsePaymentCheckoutResult {
  planData: PlanPublic | null;
  preferenceId: string | null;
  loading: boolean;
  error: string | null;
}

export const usePaymentCheckout = (planId: string): UsePaymentCheckoutResult => {
  // Garantimos que a chave de idempotência não mude em re-renderizações usando Lazy Initialization
  const [idempotencyKey] = useState(() => self.crypto.randomUUID());

  const [planData, setPlanData] = useState<PlanPublic | null>(null);
  const [preferenceId, setPreferenceId] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!planId) {
      setLoading(false);
      return;
    }

    let isMounted = true;

    const setupCheckout = async () => {
      setLoading(true);
      setError(null);

      try {
        // 1. Busca os dados consolidados do plano a partir do backend .NET 8
        const fetchedPlan = await PublicPlansService.getPlanById(planId);

        if (!fetchedPlan) {
          throw new Error('Plano não encontrado.');
        }

        // 2. Com os dados confirmados e íntegros, gera a preferência passando a idempotencyKey
        const generatedPreferenceId = await PreferenceService.createPreference(planId, idempotencyKey);

        if (isMounted) {
          setPlanData(fetchedPlan);
          setPreferenceId(generatedPreferenceId);
        }
      } catch (err: any) {
        if (isMounted) {
          setError(err.message || 'Erro ao inicializar o checkout do plano.');
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    setupCheckout();

    return () => {
      isMounted = false;
    };
  }, [planId, idempotencyKey]);

  return { planData, preferenceId, loading, error };
};
