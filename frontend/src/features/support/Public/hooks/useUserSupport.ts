/**
 * Hook para usuários criarem tickets de suporte
 * Usa userSupportService
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { userSupportService } from '../services/support-user.service';
import type { CreateSupportTicketDto } from '../../shared';
import { AlertService } from '@/shared/services/alert.service';

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
        navigate('/perfil');
        return true;
      }
      return false;
    } catch (err) {
      const errorMessage = (err as Error).message || 'Erro ao enviar ticket.';
      await AlertService.error('Erro', errorMessage);
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
