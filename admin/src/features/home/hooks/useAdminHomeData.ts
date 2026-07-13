/**
 * Hook consolidado para gerenciar dados da Home na área administrativa
 * Combina leitura de dados + hooks de Hero e Services
 */

import { useState, useCallback, useEffect } from 'react';
import { adminHomeService } from '../services/adminHome.service';
import type { HeroSlideDto, ServiceDto } from '../types/home.types';
import { ApiError } from '@/shared/services/api.service';

export const useAdminHomeData = () => {
  const [heroSlides, setHeroSlides] = useState<HeroSlideDto[]>([]);
  const [services, setServices] = useState<ServiceDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async (signal?: AbortSignal, isCurrent?: () => boolean) => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await adminHomeService.getHomeContent();
      if (signal?.aborted || (isCurrent && !isCurrent())) return;
      
      setHeroSlides(data.hero || []);
      setServices(data.services || []);
    } catch (err) {
      if (signal?.aborted || (isCurrent && !isCurrent())) return;
      
      if (err instanceof ApiError) {
        setError(err.message);
        console.error(err.message);
      } else {
        const fallbackMsg = 'Erro inesperado ao carregar dados da Home.';
        setError(fallbackMsg);
        console.error(fallbackMsg, err);
      }
    } finally {
      if (!isCurrent || isCurrent()) {
        setIsLoading(false);
      }
    }
  }, []);

  // Busca automática ao montar o componente com proteção contra Race Conditions
  useEffect(() => {
    const controller = new AbortController();
    let isCurrent = true;

    fetchData(controller.signal, () => isCurrent);

    return () => {
      isCurrent = false;
      controller.abort();
    };
  }, [fetchData]);

  return {
    heroSlides,
    services,
    isLoading,
    error,
    refreshData: () => fetchData()
  };
};
