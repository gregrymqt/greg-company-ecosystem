export type IntegrationPlatform = 'SHOPIFY' | 'NUVEMSHOP';

/**
 * ============================================================================
 * CONTRASTES GLOBAIS E COMPARTILHADOS (SHARED TYPES)
 * ============================================================================
 */

export interface FallbackCsvResponse {
  status: 'fallback_csv';
  message: string;
  download_url: string;
}

// Resposta unificada para interceptar retornos normais (HTTP 201) ou fallbacks (HTTP 202)
export type SyncResponse<T> = T | FallbackCsvResponse;


/**
 * ============================================================================
 * CONTRATOS DO PROVEDOR: SHOPIFY (GRAPHQL STABLE)
 * ============================================================================
 */

export interface ShopifyFileInput {
  originalSource: string;
  alt?: string | null;
  filename?: string | null;
  contentType?: 'IMAGE' | 'VIDEO' | 'EXTERNAL_VIDEO';
}

export interface ShopifyOptionValueInput {
  optionName: string;
  name: string;
}

export interface ShopifyVariantInput {
  price: string; // O preço no Shopify GraphQL é tratado como string decimal
  sku?: string | null;
  inventoryItem?: Record<string, any> | null;
  optionValues: ShopifyOptionValueInput[];
  file?: ShopifyFileInput | null;
}

export interface ShopifyProductOptionInput {
  name: string;
  values: Array<{ name: string }>;
}

export interface ShopifyProductSetInput {
  title: string;
  descriptionHtml: string;
  vendor: string;
  productType?: string | null;
  status: 'ACTIVE' | 'ARCHIVED' | 'DRAFT';
  productOptions?: ShopifyProductOptionInput[];
  variants?: ShopifyVariantInput[];
  files?: ShopifyFileInput[];
  seoTitle?: string | null;
  seoDescription?: string | null;
  tags?: string | null;
}

// Resposta da Query de Listagem (Relay Specification)
export interface ShopifyProductNode {
  id: string; // GID (ex: "gid://shopify/Product/...")
  title: string;
  vendor: string;
  status: 'ACTIVE' | 'ARCHIVED' | 'DRAFT';
  productType: string | null;
}

export interface ShopifyProductEdge {
  cursor: string;
  node: ShopifyProductNode;
}

export interface ShopifyPageInfo {
  hasNextPage: boolean;
  endCursor: string | null;
}

export interface ShopifyProductListResponse {
  edges: ShopifyProductEdge[];
  pageInfo: ShopifyPageInfo;
}

// Payloads de Atualização e Mídia (PUT e POST /media)
export interface ShopifyMediaPayload {
  image_urls: string[];
  alt_text?: string;
}

export interface ShopifyProductUpdatePayload {
  title?: string;
  handle?: string;
  vendor?: string;
  productType?: string;
  status?: 'ACTIVE' | 'ARCHIVED' | 'DRAFT';
  tags?: string[];
  seo?: {
    title?: string;
    description?: string;
  };
  new_images?: ShopifyFileInput[]; // Interceptado e removido dinamicamente no router
}


/**
 * ============================================================================
 * CONTRATOS DO PROVEDOR: NUVEMSHOP (REST API)
 * ============================================================================
 */

export interface NuvemshopLocalizedString {
  pt: string;
}

export interface NuvemshopVariantRequest {
  price?: number;
  compare_at_price?: number;
  stock?: number;
  sku?: string | null;
  weight?: number | null;
  width?: number | null;
  height?: number | null;
  depth?: number | null;
}

export interface NuvemshopImageRequest {
  src: string;
  alt?: NuvemshopLocalizedString | null;
}

export interface NuvemshopProductRequest {
  handle: NuvemshopLocalizedString;
  name: NuvemshopLocalizedString;
  description: NuvemshopLocalizedString;
  seo_title?: NuvemshopLocalizedString | null;
  seo_description?: NuvemshopLocalizedString | null;
  published?: boolean;
  free_shipping?: boolean;
  requires_shipping?: boolean;
  brand?: string | null;
  categories?: number[];
  tags?: string | null;
  variants: NuvemshopVariantRequest[];
  images?: NuvemshopImageRequest[];
}

// Payload para a rota de lote (PATCH /products/stock-price)
export interface NuvemshopBatchUpdateItem {
  variant_id?: number; // Se houver variantes específicas internas
  sku?: string;        // Mapeamento via SKU aceito no service
  price?: number;
  stock?: number;
}