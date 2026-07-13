/**
 * Hook para gerenciar Services na área administrativa
 * CRUD completo (sem upload de arquivo)
 */

import { useState, useCallback } from 'react';
import { adminHomeService } from '@/features/home/services/adminHome.service';
import type { ServiceFormValues } from '@/features/home/types/home.types';
import { AlertService } from '@/shared/services/alert.service';
import { ApiError } from '@/shared/services/api.service';

export const useAdminServices = () => {
  const [isLoading, setIsLoading] = useState(false);

  /**
   * Cria novo Service
   */
  const createService = useCallback(
    async (data: ServiceFormValues, onSuccess?: () => void) => {
      setIsLoading(true);
      try {
        await adminHomeService.createService(data);
        await AlertService.success('Serviço Criado!', 'O novo card de serviço já está visível.');
        if (onSuccess) onSuccess();
      } catch (error) {
        if (error instanceof ApiError) {
          AlertService.error('Atenção', error.message);
        } else {
          AlertService.error('Atenção', 'Erro ao criar o serviço.');
        }
        console.error(error);
      } finally {
        setIsLoading(false);
      }
    },
    []
  );

  /**
   * Atualiza Service existente
   */
  const updateService = useCallback(
    async (id: number, data: ServiceFormValues, onSuccess?: () => void) => {
      setIsLoading(true);
      try {
        await adminHomeService.updateService(id, data);
        AlertService.success('Serviço Atualizado', 'Dados salvos.');
        if (onSuccess) onSuccess();
      } catch (error) {
        if (error instanceof ApiError) {
          AlertService.error('Erro', error.message);
        } else {
          AlertService.error('Erro', 'Falha ao salvar as alterações.');
        }
        console.error(error);
      } finally {
        setIsLoading(false);
      }
    },
    []
  );

  /**
   * Deleta Service com confirmação
   */
  const deleteService = useCallback(
    async (id: number, onSuccess?: () => void) => {
      const { isConfirmed } = await AlertService.confirm(
        'Excluir Serviço?',
        'Este card não aparecerá mais na página inicial.',
        'Sim, excluir'
      );

      if (!isConfirmed) return;

      setIsLoading(true);
      try {
        await adminHomeService.deleteService(id);
        AlertService.success('Excluído', 'Serviço removido.');
        if (onSuccess) onSuccess();
      } catch (error) {
        if (error instanceof ApiError) {
          AlertService.error('Erro', error.message);
        } else {
          AlertService.error('Erro', 'Não foi possível excluir o serviço.');
        }
        console.error(error);
      } finally {
        setIsLoading(false);
      }
    },
    []
  );

  return {
    isLoading,
    createService,
    updateService,
    deleteService
  };
};
