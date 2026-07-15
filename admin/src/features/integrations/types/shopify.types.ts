// admin/src/features/integrations/types/shopify.types.ts

export interface ShopifyOptionValueInput {
  optionName: string;
  name: string;
}

export interface ShopifyFileInput {
  originalSource: string;
  alt?: string;
  filename?: string;
  contentType: "IMAGE" | "VIDEO" | "EXTERNAL_VIDEO";
}

export interface ShopifyVariantInput {
  price: string; // Shopify GraphQL lida com preços em String
  sku?: string;
  inventoryItem?: Record<string, any>;
  optionValues: ShopifyOptionValueInput[];
  file?: ShopifyFileInput;
}

export interface ShopifyProductOptionInput {
  name: string;
  values: Array<{ name: string }>;
}

// Input para criação global (productSet - Stable 2024-04)
export interface ShopifyProductSetInput {
  tenant_id: string;
  title: string;
  descriptionHtml: string;
  vendor: string;
  productType?: string;
  status: "ACTIVE" | "ARCHIVED" | "DRAFT";
  productOptions?: ShopifyProductOptionInput[];
  variants?: ShopifyVariantInput[];
  files?: ShopifyFileInput[];
  seoTitle?: string;
  seoDescription?: string;
  tags?: string;
}

// Input para atualizações granulares de IA
export interface ShopifySEOInput {
  title?: string;
  description?: string;
}

export interface ShopifyProductUpdateInput {
  tenant_id: string;
  id: string; // GID do Produto (ex: gid://shopify/Product/123456)
  title?: string;
  handle?: string;
  vendor?: string;
  productType?: string;
  status?: "ACTIVE" | "ARCHIVED" | "DRAFT";
  tags?: string[];
  seo?: ShopifySEOInput;
}

// Erros de validação de negócio retornados pela Shopify
export interface ShopifyUserError {
  field: string[];
  message: string;
}

// Parametrização de paginação baseada em cursor do Trello
export interface ShopifyPaginationParams {
  first: number;
  after?: string; // Cursor Base64
}

export interface ShopifyPageInfo {
  hasNextPage: boolean;
  endCursor?: string;
}

// Envelope das chamadas HTTP POST para o endpoint GraphQL
export interface ShopifyGraphQLRequest<TVariables> {
  query: string;
  variables: TVariables;
}