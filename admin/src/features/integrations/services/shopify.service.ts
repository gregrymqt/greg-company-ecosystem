// admin/src/features/integrations/services/shopify.service.ts

import { ApiService } from "@/shared/services/api.service"; // Ajuste o path conforme seu projeto
import type { ShopifyPageInfo, ShopifyProductSetInput, ShopifyProductUpdateInput } from "../types";

// Interface para mapear o retorno estruturado da listagem GraphQL da Shopify
export interface ShopifyListProductsResponse {
  edges: Array<{
    cursor: string;
    node: {
      id: string;
      title: string;
      vendor: string;
      status: "ACTIVE" | "ARCHIVED" | "DRAFT";
      productType: string;
    };
  }>;
  pageInfo: ShopifyPageInfo;
}

export const ShopifyService = {
  /**
   * Busca a lista de produtos da Shopify usando paginação baseada em cursor (after).
   */
  listProducts: async (first: number = 10, after?: string): Promise<ShopifyListProductsResponse> => {
    let endpoint = `/shopify/products?first=${first}`;
    if (after) {
      endpoint += `&after=${encodeURIComponent(after)}`;
    }
    return await ApiService.get<ShopifyListProductsResponse>(endpoint);
  },

  /**
   * Dispara a mutação declarativa productSet enviando o payload bruto para o Shopify.
   */
  syncProduct: async (productData: ShopifyProductSetInput): Promise<unknown> => {
    return await ApiService.post<unknown, ShopifyProductSetInput>("/shopify/products", productData);
  },

  /**
   * Injeta novas imagens otimizadas para SEO e acessibilidade diretamente no GID do produto.
   */
  addProductMedia: async (productId: string, imageUrls: string[], altText?: string): Promise<unknown> => {
    const payload = { image_urls: imageUrls, alt_text: altText };
    // O ID global da Shopify vem como 'gid://shopify/Product/12345', precisamos encodar para rotas URL de forma segura
    const encodedId = encodeURIComponent(productId);
    return await ApiService.post<unknown, typeof payload>(`/shopify/products/${encodedId}/media`, payload);
  },

  /**
   * Atualiza síncronamente o copywriting (SEO, tags) e anexa mídias adicionais ao produto.
   */
  updateProduct: async (productId: string, updatePayload: ShopifyProductUpdateInput): Promise<unknown> => {
    const encodedId = encodeURIComponent(productId);
    return await ApiService.put<unknown, ShopifyProductUpdateInput>(`/shopify/products/${encodedId}`, updatePayload);
  },

  /**
   * Deleta permanentemente o produto do catálogo utilizando o seu Global ID (GID).
   */
  deleteProduct: async (productId: string): Promise<void> => {
    const encodedId = encodeURIComponent(productId);
    return await ApiService.delete<void>(`/shopify/products/${encodedId}`);
  }
};