import { useState, useCallback } from "react";
import { NuvemshopService, ShopifyService } from "../services";
import { AlertService } from "@/shared/services/alert.service";
import type { 
  Product,
  NuvemshopProductRequest, 
  ShopifyProductSetInput, 
  ShopifyProductUpdateInput 
} from "../types";

// Tipos para gerenciar o estado do alerta amarelo (Fallback 202) do seu Trello
export interface FallbackInfo {
  message: string;
  downloadUrl: string;
}

export type IntegrationProvider = "nuvemshop" | "shopify";

export const useIntegrations = (activeProvider: IntegrationProvider) => {
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [fallbackInfo, setFallbackInfo] = useState<FallbackInfo | null>(null);

  // Estados de listagem e paginação (Cursor)
  const [products, setProducts] = useState<Product[]>([]);
  const [pageInfo, setPageInfo] = useState<{ hasNextPage: boolean; endCursor: string | null }>({ hasNextPage: false, endCursor: null });

  const notifyError = (msg: string) => AlertService.notify('Erro', msg, 'error');
  const notifySuccess = (msg: string) => AlertService.notify('Sucesso', msg, 'success');
  const notifyWarning = (msg: string) => AlertService.notify('Atenção', msg, 'warning');

  /**
   * 0. AÇÃO: Buscar Produtos (Lista Paginada)
   */
  const fetchProducts = useCallback(
    async (first = 10, after?: string) => {
      setIsLoading(true);
      try {
        if (activeProvider === 'shopify') {
          const response = await ShopifyService.listProducts(first, after);
          
          // Mapeamento simples do GraphQL node para a interface Product do front-end
          const fetchedProducts = response.edges.map(e => ({
            _id: e.node.id,
            sku: e.node.id, // Fallback caso não haja SKU na query básica
            title: e.node.title,
            status: e.node.status,
          } as unknown as Product));

          setProducts(prev => after ? [...prev, ...fetchedProducts] : fetchedProducts);
          setPageInfo({ 
            hasNextPage: response.pageInfo.hasNextPage, 
            endCursor: response.pageInfo.endCursor ?? null 
          });
        } else {
          // Implementação futura ou equivalente para Nuvemshop (que usa page/per_page)
          setProducts([]); 
        }
      } catch (error: unknown) {
        const err = error as Error;
        notifyError(err.message || "Erro ao buscar a lista de produtos.");
      } finally {
        setIsLoading(false);
      }
    },
    [activeProvider]
  );

  /**
   * 1. AÇÃO: Forçar Sincronização (POST)
   */
  const syncProduct = useCallback(
    async (productData: NuvemshopProductRequest | ShopifyProductSetInput): Promise<unknown> => {
      setIsLoading(true);
      setFallbackInfo(null); // Reseta o estado do fallback antes de tentar
      
      try {
        const response =
          activeProvider === "nuvemshop"
            ? await NuvemshopService.createProduct(productData as NuvemshopProductRequest)
            : await ShopifyService.syncProduct(productData as ShopifyProductSetInput);

        // Tratamento do Fallback de CSV (HTTP 202) mapeado no seu backend Python
        if (response?.status === "fallback_csv") {
          setFallbackInfo({
            message: response.message,
            downloadUrl: response.download_url,
          });
          notifyWarning("Sincronização direta falhou. CSV de contingência gerado.");
          return response;
        }

        notifySuccess("Produto sincronizado com sucesso na loja!");
        return response;
      } catch (error: unknown) {
        const err = error as Error;
        notifyError(err.message || "Erro ao sincronizar o produto.");
        throw error;
      } finally {
        setIsLoading(false);
      }
    },
    [activeProvider]
  );

  /**
   * 2. AÇÃO: Atualizar Cópia da IA (PUT/PATCH)
   */
  const updateProduct = useCallback(
    async (productId: string | number, updateData: Partial<NuvemshopProductRequest> | ShopifyProductUpdateInput): Promise<unknown> => {
      setIsLoading(true);
      try {
        const response =
          activeProvider === "nuvemshop"
            ? await NuvemshopService.updateProductMetadata(Number(productId), updateData as Partial<NuvemshopProductRequest>)
            : await ShopifyService.updateProduct(String(productId), updateData as ShopifyProductUpdateInput);
            
        notifySuccess("Metadados atualizados com sucesso!");
        return response;
      } catch (error: unknown) {
        const err = error as Error;
        notifyError(err.message || "Falha ao atualizar metadados.");
        throw error;
      } finally {
        setIsLoading(false);
      }
    },
    [activeProvider]
  );

  /**
   * 3. AÇÃO: Excluir da Loja (DELETE)
   */
  const deleteProduct = useCallback(
    async (productId: string | number): Promise<void> => {
      setIsLoading(true);
      try {
        if (activeProvider === "nuvemshop") {
          await NuvemshopService.deleteProduct(Number(productId));
        } else {
          await ShopifyService.deleteProduct(String(productId));
        }
        notifySuccess("Produto removido definitivamente da loja.");
      } catch (error: unknown) {
        const err = error as Error;
        notifyError(err.message || "Erro ao excluir o produto da loja.");
        throw error;
      } finally {
        setIsLoading(false);
      }
    },
    [activeProvider]
  );

  /**
   * 4. AÇÃO EXCLUSIVA: Atualização em Lote (Nuvemshop)
   */
  const batchUpdateNuvemshop = useCallback(
    async (batchData: Array<{ product_id?: number; variant_id?: number; price?: number; stock?: number; sku?: string }>): Promise<unknown> => {
      if (activeProvider !== "nuvemshop") {
        notifyError("A atualização em lote nativa só é suportada para a Nuvemshop.");
        return;
      }
      setIsLoading(true);
      try {
        const response = await NuvemshopService.updateStockPriceBatch(batchData);
        notifySuccess(`${batchData.length} variantes atualizadas com sucesso!`);
        return response;
      } catch (error: unknown) {
        const err = error as Error;
        notifyError(err.message || "Falha no ajuste massivo.");
        throw error;
      } finally {
        setIsLoading(false);
      }
    },
    [activeProvider]
  );

  return {
    products,
    pageInfo,
    fetchProducts,
    isLoading,
    fallbackInfo,
    clearFallback: () => setFallbackInfo(null),
    syncProduct,
    updateProduct,
    deleteProduct,
    batchUpdateNuvemshop,
  };
};