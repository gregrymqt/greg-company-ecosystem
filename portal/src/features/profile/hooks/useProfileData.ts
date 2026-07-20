/**
 * Hook para gerenciar dados do perfil do usuário
 * Usa userAccountService (UserAccountController)
 */

import { useState, useEffect, useCallback } from 'react';
import { userAccountService } from '../services/user-account.service';
import type { UserProfileDto } from '../types/profile.types';
import { AlertService } from '@/shared/services/alert.service';

export const useProfileData = () => {
  const [profile, setProfile] = useState<UserProfileDto | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const fetchProfile = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await userAccountService.getProfile();
      setProfile(data);
    } catch (err) {
      const errorMessage = (err as Error).message || 'Erro ao buscar perfil';
      setError(errorMessage);
      AlertService.error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchProfile();
  }, [fetchProfile]);

  return {
    profile,
    loading,
    error,
    refetch: fetchProfile
  };
};
