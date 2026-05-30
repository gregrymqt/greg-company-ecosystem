/**
 * Hook para gerenciar Hero Slides na área administrativa
 * CRUD completo com upload de imagem
 */

import { useState, useCallback } from 'react';
import { adminHomeService } from '@/features/home/Admin/services/home.service';
import type { HeroFormValues } from '@/features/home/shared/types/home.types';
import { AlertService } from '@/shared/services/alert.service';

export const useAdminHero = () => {
  const [isLoading, setIsLoading] = useState(false);

  /**
   * Cria novo Hero Slide
   */
  const createHero = useCallback(
    async (data: HeroFormValues, onSuccess?: () => void) => {
      setIsLoading(true);
      try {
        await adminHomeService.createHero(data);
        await AlertService.success('Sucesso!', 'Novo slide adicionado ao Hero.');
        if (onSuccess) onSuccess();
      } catch (error) {
        AlertService.error('Erro ao criar', 'Não foi possível salvar o slide. Verifique os dados.');
        console.error(error);
      } finally {
        setIsLoading(false);
      }
    },
    []
  );

  /**
   * Atualiza Hero Slide existente
   */
  const updateHero = useCallback(
    async (id: number, data: HeroFormValues, onSuccess?: () => void) => {
      setIsLoading(true);
      try {
        await adminHomeService.updateHero(id, data);
        AlertService.notify('Atualizado', 'As alterações no slide foram salvas.', 'success');
        if (onSuccess) onSuccess();
      } catch (error) {
        AlertService.error('Erro', 'Falha ao atualizar o slide.');
        console.error(error);
      } finally {
        setIsLoading(false);
      }
    },
    []
  );

  /**
   * Deleta Hero Slide com confirmação
   */
  const deleteHero = useCallback(async (id: number, onSuccess?: () => void) => {
    const { isConfirmed } = await AlertService.confirm(
      'Remover Slide?',
      'Essa ação removerá a imagem e o texto do carrossel permanentemente.',
      'Sim, remover'
    );

    if (!isConfirmed) return;

    setIsLoading(true);
    try {
      await adminHomeService.deleteHero(id);
      AlertService.notify('Removido', 'Slide removido com sucesso.', 'info');
      if (onSuccess) onSuccess();
    } catch (error) {
      AlertService.error('Erro', 'Não foi possível remover este item.');
      console.error(error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  return {
    isLoading,
    createHero,
    updateHero,
    deleteHero
  };
};
