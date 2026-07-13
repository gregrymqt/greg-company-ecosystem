// admin/src/features/integrations/services/nuvemshop.service.ts

import { ApiService } from "@/shared/services/api.service"; // Ajuste o path conforme seu projeto
import type { NuvemshopProductRequest } from "../types";

export const NuvemshopService = {
  /**
   * Envia o produto refinado pela IA para ser criado na Nuvemshop.
   * Suporta o retorno 202 com link do CSV de fallback em caso de indisponibilidade da API.
   */
  createProduct: async (product: NuvemshopProductRequest): Promise<any> => {
    return await ApiService.post<any, NuvemshopProductRequest>("/nuvemshop/products", product);
  },

  /**
   * Obtém os detalhes de um produto na Nuvemshop pelo ID interno da plataforma.
   */
  getProductById: async (productId: number): Promise<any> => {
    return await ApiService.get<any>(`/nuvemshop/products/${productId}`);
  },

  /**
   * Busca um produto pelo SKU antes de disparar uma sincronização (evita duplicidade).
   */
  getProductBySku: async (sku: string): Promise<any> => {
    return await ApiService.get<any>(`/nuvemshop/products/sku/${sku}`);
  },

  /**
   * Modifica dados textuais e de SEO (título, descrição em HTML) gerados pela IA.
   */
  updateProductMetadata: async (productId: number, updateData: Partial<NuvemshopProductRequest>): Promise<any> => {
    return await ApiService.put<any, Partial<NuvemshopProductRequest>>(`/nuvemshop/products/${productId}`, updateData);
  },

  /**
   * Atualização massiva de preço e estoque de até 50 variantes por lote.
   * Aciona o motor síncrono do inventário do seu Trello.
   */
  updateStockPriceBatch: async (batchData: Array<Record<string, any>>): Promise<any[]> => {
    if (batchData.length > 50) {
      throw new Error("A API da Nuvemshop aceita no máximo 50 variantes por lote.");
    }
    return await ApiService.patch<any[], Array<Record<string, any>>>("/nuvemshop/products/stock-price", batchData);
  },

  /**
   * Remove o produto permanentemente do catálogo da Nuvemshop.
   */
  deleteProduct: async (productId: number): Promise<void> => {
    return await ApiService.delete<void>(`/nuvemshop/products/${productId}`);
  }
};