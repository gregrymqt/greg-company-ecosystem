import { useState } from 'react';
import { userAccountService } from '../services/user-account.service';
import { AlertService } from '@/shared/services/alert.service';
import { useAuth } from '@/features/auth/hooks/useAuth';

export const useAvatarUpdate = () => {
  const [uploading, setUploading] = useState(false);
  const { refreshSession } = useAuth(); // Para forçar o reload da sessão no top level e refletir a nova foto no header

  const updateAvatar = async (file: File) => {
    setUploading(true);
    try {
      await userAccountService.updateAvatar(file);
      AlertService.success('Sucesso', 'Foto atualizada com sucesso!');
      await refreshSession(); // Atualiza a sessão que vai jogar a nova imagem na store da aplicação
    } catch (error) {
      console.error(error);
      AlertService.error('Erro', 'Não foi possível atualizar a foto.');
    } finally {
      setUploading(false);
    }
  };

  return {
    uploading,
    updateAvatar
  };
};
