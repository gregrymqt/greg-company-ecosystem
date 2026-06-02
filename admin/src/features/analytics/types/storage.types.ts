export interface FileCategoryBreakdown {
  FeatureCategoria: string;
  TotalFiles: number;
  TotalBytes: number;
  TotalGB: number;
  PercentageOfTotal: number;
  AvgFileSizeMB: number;
}

export interface StorageStats {
  TotalFiles: number;
  TotalBytes: number;
  TotalGB: number;
  TotalMB: number;
  CategoryBreakdown: FileCategoryBreakdown[];
  LargestCategory: string | null;
  SmallestCategory: string | null;
}

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

export interface FileDetail {
  Id: number;
  FileName: string;
  FeatureCategoria: string;
  TamanhoBytes: number;
  SizeMB: number;
  CriadoEm: string;
  ModificadoEm: string | null;
}

export interface StorageWSPayload {
  stats: StorageStats;
  type: "Initial" | "Manual" | "Auto";
}
