/**
 * Hook para usuários criarem tickets de suporte
 * Usa userSupportService
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { userSupportService } from '../services/support-user.service';
import type { CreateSupportTicketDto } from '../types/support.types';
import { AlertService } from '@/shared/services/alert.service';
import { ApiError } from '@/shared/services/api.service';

export const useUserSupport = () => {
  const [loading, setLoading] = useState<boolean>(false);
  const navigate = useNavigate();

  const createTicket = async (payload: CreateSupportTicketDto): Promise<boolean> => {
    setLoading(true);
    try {
      const response = await userSupportService.createTicket(payload);
      
      if (response.success) {
        await AlertService.success(
          'Recebido!',
          response.message || 'Seu ticket foi criado. Em breve entraremos em contato.'
        );
        // Redirecionamento SPA nativo (sem reload)
        navigate('/perfil');
        return true;
      } else {
        // Tratamento elegante caso success venha false no DTO da API
        await AlertService.error(
          'Não foi possível criar',
          response.message || 'Houve um problema interno ao processar seu ticket.'
        );
        return false;
      }
    } catch (err) {
      // Captura segura e tipada do erro HTTP
      const errorMessage = err instanceof ApiError ? err.message : 'Erro ao tentar enviar ticket. Tente novamente mais tarde.';
      await AlertService.error('Erro de Conexão', errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  };

  return {
    createTicket,
    loading
  };
};
