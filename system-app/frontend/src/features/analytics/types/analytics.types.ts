/**
 * Analytics Feature - Type Definitions
 * Interfaces TypeScript para a feature de Analytics
 */

// Métricas de produto individual
export interface ProductMetric {
  id: string;
  name: string;
  stock: number;
  status: 'OK' | 'CRITICO' | 'ESGOTADO' | 'REPOR';
  price: number;
  category: string;
  thumbnail?: string;
  lastUpdate: string;
}

// Dados agregados do dashboard
export interface AnalyticsDashboard {
  totalProducts: number;
  criticalProducts: number;
  outOfStockProducts: number;
  totalRevenue: number;
  averageStock: number;
  lastSync: string;
}

// Resposta da API do FastAPI (porta 8888)
export interface AnalyticsApiResponse {
  success: boolean;
  data: {
    dashboard: AnalyticsDashboard;
    products: ProductMetric[];
  };
  timestamp: string;
}

// Filtros para a busca de analytics
export interface AnalyticsFilters {
  status?: 'OK' | 'CRITICO' | 'ESGOTADO' | 'REPOR';
  category?: string;
  minStock?: number;
  maxStock?: number;
}

// Estado do hook de analytics
export interface UseAnalyticsState {
  dashboard: AnalyticsDashboard | null;
  products: ProductMetric[];
  isLoading: boolean;
  error: string | null;
  filters: AnalyticsFilters;
}
