/**
 * Analytics Feature - Service Layer
 * Comunicação com FastAPI na porta 8888
 * Herda funcionalidades do ApiService base
 */

import { ApiService } from "@/shared/services/api.service";
import type { 
  AnalyticsApiResponse, 
  AnalyticsDashboard, 
  ProductMetric,
  AnalyticsFilters 
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
};
