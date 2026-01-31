/**
 * Hook para atualização de avatar
 * Usa userAccountService (UserAccountController)
 */

import { useState } from 'react';
import { userAccountService } from '../services/user-account.service';
import type { AvatarUpdateResponse } from '../../shared';
import { AlertService } from '@/shared/services/alert.service';

export const useAvatarUpdate = () => {
  const [uploading, setUploading] = useState<boolean>(false);

  const updateAvatar = async (file: File): Promise<AvatarUpdateResponse | null> => {
    setUploading(true);
    try {
      const response = await userAccountService.updateAvatar(file);
      AlertService.success(response.message || 'Avatar atualizado com sucesso!');
      return response;
    } catch (err) {
      const errorMessage = (err as Error).message || 'Erro ao atualizar avatar';
      AlertService.error(errorMessage);
      return null;
    } finally {
      setUploading(false);
    }
  };

  return {
    updateAvatar,
    uploading
  };
};
