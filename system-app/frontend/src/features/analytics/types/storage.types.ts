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
  LargestCategory?: string;
  SmallestCategory?: string;
}

export interface StorageWSPayload {
  stats: StorageStats;
  type: "Initial" | "Manual" | "Auto";
}