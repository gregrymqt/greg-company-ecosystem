/**
 * Analytics Feature - Service Layer
 * Comunicação com FastAPI na porta 8888 (legacy) e porta 8000 (BI Dashboard Python)
 * Herda funcionalidades do ApiService base
 */

import { ApiService, BiApiService } from "@/shared/services/api.service";
import type { 
  AnalyticsApiResponse, 
  AnalyticsDashboard, 
  ProductMetric,
  AnalyticsFilters,
  StorageStats,
  StorageGrowthTrend,
  FileDetail
} from "@/features/analytics/types/analytics.types";

// Base URL do FastAPI (diferente do backend principal)
const ANALYTICS_BASE_URL = 'http://localhost:8888';
const ENDPOINT = '/api/analytics';

/**
 * Serviço de Analytics
 * Utiliza o ApiService para requisições com tratamento de erros e MCP reporting
 */
export const AnalyticsService = {
  
  /**
   * Busca dados completos do dashboard de analytics
   * @returns Dashboard com métricas agregadas e lista de produtos
   */
  getDashboard: async (): Promise<AnalyticsApiResponse> => {
    // Usa fetch customizado pois o FastAPI está em porta diferente
    const response = await fetch(`${ANALYTICS_BASE_URL}${ENDPOINT}/dashboard`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Erro ao buscar analytics: ${response.statusText}`);
    }

    // Report to MCP
    fetch("http://localhost:8888/log", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ 
        source: "Analytics", 
        url: `${ANALYTICS_BASE_URL}${ENDPOINT}/dashboard`, 
        method: "GET", 
        status: response.status 
      }),
    }).catch(() => {}); // Ignora se MCP estiver offline

    return await response.json();
  },

  /**
   * Busca produtos filtrados por critérios
   * @param filters Filtros de busca
   * @returns Lista de produtos filtrados
   */
  getProducts: async (filters?: AnalyticsFilters): Promise<ProductMetric[]> => {
    const queryParams = new URLSearchParams();
    
    if (filters) {
      if (filters.status) queryParams.append('status', filters.status);
      if (filters.category) queryParams.append('category', filters.category);
      if (filters.minStock !== undefined) queryParams.append('minStock', filters.minStock.toString());
      if (filters.maxStock !== undefined) queryParams.append('maxStock', filters.maxStock.toString());
    }

    const url = `${ANALYTICS_BASE_URL}${ENDPOINT}/products${queryParams.toString() ? `?${queryParams.toString()}` : ''}`;
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Erro ao buscar produtos: ${response.statusText}`);
    }

    // Report to MCP
    fetch("http://localhost:8888/log", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ 
        source: "Analytics", 
        url, 
        method: "GET", 
        status: response.status 
      }),
    }).catch(() => {});

    const data = await response.json();
    return data.products || [];
  },

  /**
   * Sincroniza dados com a fonte externa (DummyJSON/Notion/Rows)
   * @returns Status da sincronização
   */
  syncData: async (): Promise<{ success: boolean; message: string }> => {
    const url = `${ANALYTICS_BASE_URL}${ENDPOINT}/sync`;
    
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Erro ao sincronizar dados: ${response.statusText}`);
    }

    // Report to MCP
    fetch("http://localhost:8888/log", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ 
        source: "Analytics", 
        url, 
        method: "POST", 
        status: response.status 
      }),
    }).catch(() => {});

    return await response.json();
  },

  /**
   * Exporta dados para Excel
   * @returns Blob do arquivo Excel
   */
  exportToExcel: async (): Promise<Blob> => {
    const url = `${ANALYTICS_BASE_URL}${ENDPOINT}/export/excel`;
    
    const response = await fetch(url, {
      method: 'GET',
    });

    if (!response.ok) {
      throw new Error(`Erro ao exportar para Excel: ${response.statusText}`);
    }

    // Report to MCP
    fetch("http://localhost:8888/log", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ 
        source: "Analytics", 
        url, 
        method: "GET", 
        status: response.status 
      }),
    }).catch(() => {});

    return await response.blob();
  },

  // ==================== STORAGE ANALYTICS (BI DASHBOARD PYTHON - PORTA 8000) ====================

  /**
   * Busca visão geral de armazenamento com breakdown por categoria
   * @returns StorageStats com estatísticas totais e breakdown
   */
  getStorageOverview: async (): Promise<StorageStats> => {
    return await BiApiService.get<StorageStats>('/api/storage/overview');
  },

  /**
   * Busca os maiores arquivos do sistema
   * @param limit Quantidade de arquivos a retornar (padrão: 10, máx: 100)
   * @returns Lista de arquivos ordenada por tamanho
   */
  getLargestFiles: async (limit: number = 10): Promise<{ TotalResults: number; Files: FileDetail[] }> => {
    return await BiApiService.get<{ TotalResults: number; Files: FileDetail[] }>(
      `/api/storage/largest-files?limit=${limit}`
    );
  },

  /**
   * Busca arquivos de uma categoria específica
   * @param categoria Nome da categoria (ex: 'Videos', 'Imagens')
   * @param limit Quantidade de arquivos (padrão: 50, máx: 500)
   * @returns Lista de arquivos filtrada por categoria
   */
  getFilesByCategory: async (
    categoria: string, 
    limit: number = 50
  ): Promise<{ Category: string; TotalResults: number; Files: FileDetail[] }> => {
    return await BiApiService.get<{ Category: string; TotalResults: number; Files: FileDetail[] }>(
      `/api/storage/by-category/${encodeURIComponent(categoria)}?limit=${limit}`
    );
  },

  /**
   * Busca tendência de crescimento de armazenamento
   * @param days Quantidade de dias para análise (padrão: 30, máx: 365)
   * @returns Dados de crescimento diário + resumo
   */
  getStorageGrowthTrend: async (days: number = 30): Promise<StorageGrowthTrend> => {
    return await BiApiService.get<StorageGrowthTrend>(`/api/storage/growth-trend?days=${days}`);
  },
};
