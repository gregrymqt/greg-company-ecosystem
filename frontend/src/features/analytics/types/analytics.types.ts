/**
 * Analytics Feature - Type Definitions
 * Interfaces TypeScript para a feature de Analytics
 */

// Define status constants for reuse and type safety
export const PRODUCT_STATUS = {
  OK: 'OK',
  CRITICO: 'CRITICO',
  ESGOTADO: 'ESGOTADO',
  REPOR: 'REPOR'
} as const;

export type ProductStatus = typeof PRODUCT_STATUS[keyof typeof PRODUCT_STATUS];

// Métricas de produto individual
export interface ProductMetric {
  id: string;
  name: string;
  stock: number;
  status: ProductStatus;
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
  status?: ProductStatus;
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

// ==================== STORAGE ANALYTICS (BI DASHBOARD PYTHON) ====================

// Breakdown de armazenamento por categoria
export interface FileCategoryBreakdown {
  FeatureCategoria: string;
  TotalFiles: number;
  TotalBytes: number;
  TotalGB: number;
  PercentageOfTotal: number;
  AvgFileSizeMB: number;
}

// Estatísticas gerais de armazenamento
export interface StorageStats {
  TotalFiles: number;
  TotalBytes: number;
  TotalGB: number;
  TotalMB: number;
  LargestCategory: string | null;
  SmallestCategory: string | null;
  CategoryBreakdown: FileCategoryBreakdown[];
}

// Tendência de crescimento de armazenamento
export interface StorageGrowthTrendData {
  Date: string;
  FilesAdded: number;
  GBAdded: number;
}

export interface StorageGrowthTrend {
  TrendData: StorageGrowthTrendData[];
  Summary: {
    Days: number;
    TotalGBAdded: number;
    TotalFilesAdded: number;
    AvgGBPerDay: number;
  };
}

// Detalhes individuais de arquivo
export interface FileDetail {
  Id: number;
  FileName: string;
  FeatureCategoria: string;
  TamanhoBytes: number;
  SizeMB: number;
  CriadoEm: string;
  ModificadoEm: string | null;
}
