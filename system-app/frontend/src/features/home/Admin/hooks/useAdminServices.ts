/**
 * Hook para gerenciar Services na área administrativa
 * CRUD completo (sem upload de arquivo)
 */

import { useState, useCallback } from 'react';
import { adminHomeService } from '@/features/home/Admin/services/home.service';
import type { ServiceFormValues } from '@/features/home/shared/types/home.types';
import { AlertService } from '@/shared/services/alert.service';

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
        AlertService.error('Atenção', 'Erro ao criar o serviço.');
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
        AlertService.notify('Serviço Atualizado', 'Dados salvos.', 'success');
        if (onSuccess) onSuccess();
      } catch (error) {
        AlertService.error('Erro', 'Falha ao salvar as alterações.');
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
        AlertService.notify('Excluído', 'Serviço removido.', 'info');
        if (onSuccess) onSuccess();
      } catch (error) {
        AlertService.error('Erro', 'Não foi possível excluir o serviço.');
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
