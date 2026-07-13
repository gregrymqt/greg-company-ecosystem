import { useState, useEffect, useCallback } from "react";
import { ApiError } from "@/shared/services/api.service";
import { ChargebackService } from '../services/chargeBack.service';
import type { ChargebackDetail } from '../types/chargeback.types';

export const useChargebackDetails = (chargebackId: string | null) => {
  const [details, setDetails] = useState<ChargebackDetail | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const fetchDetails = useCallback(async (isCurrentCheck: () => boolean = () => true) => {
    if (!chargebackId) return; // Não busca se não tiver ID

    setLoading(true);
    setError(null);
    setDetails(null); // Limpa dados anteriores enquanto carrega

    try {
      const data = await ChargebackService.getById(chargebackId);
      if (isCurrentCheck()) setDetails(data);
    } catch (err) {
      if (isCurrentCheck() && err instanceof ApiError) {
        console.error(err);
        setError(err.message);
      }
    } finally {
      if (isCurrentCheck()) setLoading(false);
    }
  }, [chargebackId]);

  useEffect(() => {
    let isCurrent = true;
    if (chargebackId) {
      fetchDetails(() => isCurrent);
    } else {
      setDetails(null); // Reseta se fechar o modal (id virar null)
    }
    return () => {
      isCurrent = false;
    };
  }, [fetchDetails, chargebackId]);

  return {
    details,
    loading,
    error,
    retry: () => fetchDetails(() => true),
  };
};
