import { useState, useCallback } from 'react';
import { adminSubscriptionService } from '../services/adminSubscription.service';
import { ApiError } from '@/shared/services/api.service';
import type { AdminSubscriptionDetail, AdminSubscriptionList } from '../types/subscriptions.types';
import { AlertService } from '@/shared/services/alert.service';

export const useAdminSubscription = () => {
  const [subscription, setSubscription] = useState<AdminSubscriptionDetail | null>(null);
  const [subscriptionsList, setSubscriptionsList] = useState<AdminSubscriptionList[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  // Função para buscar a assinatura (pelo ID ou Query)
  const searchSubscription = useCallback(async (query: string) => {
    if (!query) return;

    setLoading(true);
    setSubscription(null); // Limpa o estado anterior enquanto busca

    try {
      const data = await adminSubscriptionService.searchSubscription(query);
      setSubscription(data);
    } catch (err) {
      // Tenta pegar a mensagem do backend ou usa uma genérica
      const msg = err instanceof ApiError ? err.message : 'Erro ao buscar assinatura.';
      AlertService.error('Erro na Busca', msg);
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchList = useCallback(async (page = 1, pageSize = 10, statusFilter?: string, searchTerm?: string) => {
    setLoading(true);
    try {
      const result = await adminSubscriptionService.getSubscriptionsList(page, pageSize, statusFilter, searchTerm);
      setSubscriptionsList(result.data);
      setTotalCount(result.totalCount);
    } catch (err) {
      const msg = err instanceof ApiError ? err.message : 'Erro ao listar assinaturas.';
      AlertService.error('Erro na Listagem', msg);
    } finally {
      setLoading(false);
    }
  }, []);

  // Função para atualizar o Valor
  const updateValue = useCallback(async (amount: number): Promise<boolean> => {
    if (!subscription?.id) {
        AlertService.error('Erro', 'Nenhuma assinatura selecionada para atualização.');
        return false;
    }

    setLoading(true);

    try {
      const updatedData = await adminSubscriptionService.updateValue(subscription.id, amount);
      // Atualiza o estado local com os dados novos retornados pela API
      setSubscription(updatedData);
      await AlertService.success('Sucesso!', 'Assinatura atualizada com sucesso.');
      return true;
    } catch (err) {
        const msg = err instanceof ApiError ? err.message : 'Erro ao atualizar valor.';
        AlertService.error('Erro', msg);
        return false;
    } finally {
      setLoading(false);
    }
  }, [subscription]);

  // Função para atualizar o Status
  const updateStatus = useCallback(async (status: 'authorized' | 'paused' | 'cancelled'): Promise<boolean> => {
    if (!subscription?.id) {
        AlertService.error('Erro', 'Nenhuma assinatura selecionada para atualização.');
        return false;
    }

    setLoading(true);

    try {
      const updatedData = await adminSubscriptionService.updateStatus(subscription.id, status);
      // Atualiza o estado local com os dados novos retornados pela API
      setSubscription(updatedData);
      await AlertService.success('Sucesso!', 'Assinatura atualizada com sucesso.');
      return true;
    } catch (err) {
      const msg = err instanceof ApiError ? err.message : 'Erro ao atualizar status.';
      AlertService.error('Erro', msg);
      return false;
    } finally {
      setLoading(false);
    }
  }, [subscription]);

  const cancelSubscription = useCallback(async (id: string): Promise<boolean> => {
    setLoading(true);
    try {
      await adminSubscriptionService.cancelSubscription(id);
      await AlertService.success('Sucesso!', 'Assinatura cancelada com sucesso.');
      // Atualiza a lista atual se existir
      setSubscriptionsList(prev => prev.map(sub => sub.subscriptionId === id ? { ...sub, status: 'cancelled' } : sub));
      if (subscription?.id === id) {
          setSubscription({ ...subscription, status: 'cancelled' });
      }
      return true;
    } catch (err) {
      const msg = err instanceof ApiError ? err.message : 'Erro ao cancelar assinatura.';
      AlertService.error('Erro', msg);
      return false;
    } finally {
      setLoading(false);
    }
  }, [subscription]);

  return {
    subscription,
    subscriptionsList,
    totalCount,
    loading,
    searchSubscription,
    fetchList,
    updateValue,
    updateStatus,
    cancelSubscription
  };
};
