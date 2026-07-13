// admin/src/features/integrations/types/nuvemshop.types.ts

export interface NuvemshopLocalizedString {
  pt: string;
  [key: string]: string; // Flexibilidade para novos idiomas no futuro
}

export interface NuvemshopVariantRequest {
  price?: number;
  compare_at_price?: number;
  stock?: number;
  sku?: string;
  weight?: number;
  width?: number;
  height?: number;
  depth?: number;
}

export interface NuvemshopImageRequest {
  src: string;
  alt?: NuvemshopLocalizedString;
}

export interface NuvemshopProductRequest {
  handle: NuvemshopLocalizedString; // Slug único da URL
  name: NuvemshopLocalizedString;
  description: NuvemshopLocalizedString;
  seo_title?: NuvemshopLocalizedString;
  seo_description?: NuvemshopLocalizedString;
  published: boolean;
  free_shipping: boolean;
  requires_shipping: boolean;
  brand?: string;
  categories: number[];
  tags?: string; // Tags separadas por vírgula
  variants: NuvemshopVariantRequest[];
  images: NuvemshopImageRequest[];
}