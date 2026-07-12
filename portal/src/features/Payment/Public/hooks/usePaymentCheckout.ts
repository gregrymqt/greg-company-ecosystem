import { useState, useEffect } from 'react';

export interface PlanData {
  id: string;
  name: string;
  amount: number;
  frequency: string;
}

export interface UsePaymentCheckoutResult {
  planData: PlanData | null;
  preferenceId: string | null;
  loading: boolean;
  error: string | null;
}

export const usePaymentCheckout = (planId: string): UsePaymentCheckoutResult => {
  const [planData, setPlanData] = useState<PlanData | null>(null);
  const [preferenceId, setPreferenceId] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!planId) {
      setLoading(false);
      return;
    }

    let isMounted = true;

    const fetchCheckoutSetup = async () => {
      setLoading(true);
      setError(null);
      
      try {
        // O ideal é buscar os dados do plano e gerar a preferenceId no backend em uma única viagem de rede.
        // Dessa forma, garantimos a segurança (preço vem do BD) e evitamos múltiplos loadings.
        const response = await fetch(`/api/payments/checkout-setup/${planId}`);
        
        if (!response.ok) {
          throw new Error('Falha ao obter os dados do checkout');
        }

        const data = await response.json();
        
        if (isMounted) {
          setPlanData(data.plan); // Espera-se que o backend retorne o objeto plan
          setPreferenceId(data.preferenceId);
        }
      } catch (err: any) {
        if (isMounted) {
          setError(err.message || 'Erro ao carregar o checkout.');
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    fetchCheckoutSetup();

    return () => {
      isMounted = false;
    };
  }, [planId]);

  return { planData, preferenceId, loading, error };
};
