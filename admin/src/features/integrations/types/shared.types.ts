// admin/src/features/integrations/types/shared.types.ts

export type ProductStatus = 
  | "Raw"
  | "Processing"
  | "Processed"
  | "Failed"
  | "Exported";

export interface ScraperMetadata {
  source_url: string;
  last_scraped_at?: string; // ISO DateTime string
  scraper_version: string;
}

export interface Product {
  _id?: string;
  tenant_id?: string; // Fornecido opcionalmente pelo estado para rastreio
  sku: string;
  title?: string;
  description?: string;
  price?: number;
  currency: string; // Default: "BRL"
  images: string[];
  category: string; // Default: "Geral"
  attributes: Record<string, string>;
  metadata: ScraperMetadata;
  status: ProductStatus;
  created_at: string;
  updated_at?: string;
  last_error?: string;
}