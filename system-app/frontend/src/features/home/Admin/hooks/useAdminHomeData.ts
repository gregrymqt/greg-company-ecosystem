/**
 * Hook consolidado para gerenciar dados da Home na área administrativa
 * Combina leitura de dados + hooks de Hero e Services
 */

import { useState, useCallback, useEffect } from 'react';
import { publicHomeService } from '@/features/home/Public/services/home.service';
import type { HeroSlideDto, ServiceDto } from '@/features/home/shared/types/home.types';
import { ApiError } from '@/shared/services/api.service';

export const useAdminHomeData = () => {
  const [heroSlides, setHeroSlides] = useState<HeroSlideDto[]>([]);
  const [services, setServices] = useState<ServiceDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await publicHomeService.getHomeContent();
      setHeroSlides(data.hero || []);
      setServices(data.services || []);
    } catch (err) {
      const msg = err instanceof ApiError ? err.message : 'Erro ao carregar dados da Home.';
      setError(msg);
      console.error(msg);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Busca automática ao montar o componente
  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return {
    heroSlides,
    services,
    isLoading,
    error,
    refreshData: fetchData
  };
};
