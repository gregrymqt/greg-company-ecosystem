/**
 * Analytics Feature - useAnalytics Hook
 * Gerencia estado e lógica de fetch dos dados de analytics
 */

import { useState, useEffect, useCallback } from 'react';
import { AnalyticsService } from '@/features/analytics/services/analytics.service';
import type { 
  UseAnalyticsState,
  AnalyticsDashboard,
  ProductMetric,
  AnalyticsFilters,
  StorageStats
} from '@/features/analytics/types/analytics.types';
import { AlertService } from '@/shared/services/alert.service';

export const useAnalytics = () => {
  const [state, setState] = useState<UseAnalyticsState>({
    dashboard: null,
    products: [],
    isLoading: false,
    error: null,
    filters: {},
  });

  const [storageStats, setStorageStats] = useState<StorageStats | null>(null);

  /**
   * Carrega dados do dashboard
   */
  const loadDashboard = useCallback(async () => {
    setState(prev => ({ ...prev, isLoading: true, error: null }));
    
    try {
      const response = await AnalyticsService.getDashboard();
      
      setState(prev => ({
        ...prev,
        dashboard: response.data.dashboard,
        products: response.data.products,
        isLoading: false,
      }));
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao carregar analytics';
      setState(prev => ({
        ...prev,
        error: errorMessage,
        isLoading: false,
      }));
      AlertService.error('Erro', errorMessage);
    }
  }, []);

  /**
   * Carrega produtos com filtros
   */
  const loadProducts = useCallback(async (filters?: AnalyticsFilters) => {
    setState(prev => ({ ...prev, isLoading: true, error: null, filters: filters || {} }));
    
    try {
      const products = await AnalyticsService.getProducts(filters);
      
      setState(prev => ({
        ...prev,
        products,
        isLoading: false,
      }));
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao carregar produtos';
      setState(prev => ({
        ...prev,
        error: errorMessage,
        isLoading: false,
      }));
      AlertService.error('Erro', errorMessage);
    }
  }, []);

  /**
   * Sincroniza dados com fonte externa
   */
  const syncData = useCallback(async () => {
    setState(prev => ({ ...prev, isLoading: true }));
    
    try {
      const result = await AnalyticsService.syncData();
      
      if (result.success) {
        AlertService.success('Sucesso', result.message || 'Dados sincronizados com sucesso');
        await loadDashboard(); // Recarrega após sincronizar
      } else {
        AlertService.error('Erro', 'Falha na sincronização');
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao sincronizar dados';
      AlertService.error('Erro', errorMessage);
    } finally {
      setState(prev => ({ ...prev, isLoading: false }));
    }
  }, [loadDashboard]);

  /**
   * Exporta dados para Excel
   */
  const exportToExcel = useCallback(async () => {
    try {
      const blob = await AnalyticsService.exportToExcel();
      
      // Cria link temporário para download
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `analytics_${new Date().toISOString().split('T')[0]}.xlsx`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      
      AlertService.success('Sucesso', 'Exportação concluída');
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao exportar dados';
      AlertService.error('Erro', errorMessage);
    }
  }, []);

  /**
   * Atualiza filtros
   */
  const updateFilters = useCallback((newFilters: AnalyticsFilters) => {
    loadProducts(newFilters);
  }, [loadProducts]);

  /**
   * Carrega dados de armazenamento do BI Dashboard Python
   */
  const loadStorageStats = useCallback(async () => {
    try {
      const stats = await AnalyticsService.getStorageOverview();
      setStorageStats(stats);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao carregar dados de armazenamento';
      console.error(errorMessage);
      // Não exibe alerta para não poluir a UX, apenas loga
    }
  }, []);
storageStats, // Novo: dados de armazenamento
    loadDashboard,
    loadProducts,
    loadStorageStats, // Novo: recarregar storage manualmente se necessário
   * Carrega dados iniciais
   */
  useEffect(() => {
    loadDashboard();
    loadStorageStats(); // Carrega também os dados de storage
  }, [loadDashboard, loadStorageStats]);

  return {
    dashboard: state.dashboard,
    products: state.products,
    isLoading: state.isLoading,
    error: state.error,
    filters: state.filters,
    loadDashboard,
    loadProducts,
    syncData,
    exportToExcel,
    updateFilters,
  };
};
