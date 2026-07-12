import { ApiService } from "@/shared/services/api.service";
import type { ShopifyProductListResponse, SyncResponse, ShopifyProductUpdatePayload, ShopifyMediaPayload, NuvemshopProductRequest, NuvemshopBatchUpdateItem } from "../types/integration.type";

/**
 * Helper privado para injetar dinamicamente o Header de Multi-Tenant (BYOK)
 * respeitando a assinatura de RequestInit do seu ApiService.
 */
const getRequestConfig = (): RequestInit => {
    const activeTenantId = localStorage.getItem("active_tenant_id") || "default-tenant";
    return {
        headers: {
            "X-Tenant-ID": activeTenantId,
        },
    };
};

export const IntegrationService = {
    /**
     * ============================================================================
     * OPERAÇÕES SHOPIFY (GRAPHQL API ROUTER)
     * ============================================================================
     */
    shopify: {
        // GET /api/shopify/products
        listProducts: async (first = 10, after?: string | null): Promise<ShopifyProductListResponse> => {
            const params = new URLSearchParams({ first: String(first) });
            if (after) params.append("after", after);

            return ApiService.get<ShopifyProductListResponse>(
                `/shopify/products?${params.toString()}`,
                getRequestConfig()
            );
        },

        // POST /api/shopify/products (Suporta Fallback CSV 202)
        syncProduct: async (productData: Record<string, any>): Promise<SyncResponse<any>> => {
            return ApiService.post<SyncResponse<any>, Record<string, any>>(
                "/shopify/products",
                productData,
                getRequestConfig()
            );
        },

        // PUT /api/shopify/products/{product_id}
        updateProduct: async (productId: string, payload: ShopifyProductUpdatePayload): Promise<Record<string, any>> => {
            return ApiService.put<Record<string, any>, ShopifyProductUpdatePayload>(
                `/shopify/products/${encodeURIComponent(productId)}`,
                payload,
                getRequestConfig()
            );
        },

        // POST /api/shopify/products/{product_id}/media
        addProductMedia: async (productId: string, media: ShopifyMediaPayload): Promise<Record<string, any>> => {
            return ApiService.post<Record<string, any>, ShopifyMediaPayload>(
                `/shopify/products/${encodeURIComponent(productId)}/media`,
                media,
                getRequestConfig()
            );
        },

        // DELETE /api/shopify/products/{product_id}
        deleteProduct: async (productId: string): Promise<void> => {
            return ApiService.delete<void>(
                `/shopify/products/${encodeURIComponent(productId)}`,
                getRequestConfig()
            );
        }
    },

    /**
     * ============================================================================
     * OPERAÇÕES NUVEMSHOP (REST API ROUTER)
     * ============================================================================
     */
    nuvemshop: {
        // POST /api/nuvemshop/products (Suporta Fallback CSV 202)
        createProduct: async (product: NuvemshopProductRequest): Promise<SyncResponse<any>> => {
            return ApiService.post<SyncResponse<any>, NuvemshopProductRequest>(
                "/nuvemshop/products",
                product,
                getRequestConfig()
            );
        },

        // GET /api/nuvemshop/products/{product_id}
        getProductById: async (productId: number): Promise<Record<string, any>> => {
            return ApiService.get<Record<string, any>>(
                `/nuvemshop/products/${productId}`,
                getRequestConfig()
            );
        },

        // GET /api/nuvemshop/products/sku/{sku}
        getProductBySku: async (sku: string): Promise<Record<string, any>> => {
            return ApiService.get<Record<string, any>>(
                `/nuvemshop/products/sku/${encodeURIComponent(sku)}`,
                getRequestConfig()
            );
        },

        // PUT /api/nuvemshop/products/{product_id}
        updateMetadata: async (productId: number, updateData: Record<string, any>): Promise<Record<string, any>> => {
            return ApiService.put<Record<string, any>, Record<string, any>>(
                `/nuvemshop/products/${productId}`,
                updateData,
                getRequestConfig()
            );
        },

        // PATCH /api/nuvemshop/products/stock-price (Atualização massiva em lote)
        updateStockPriceBatch: async (batchData: NuvemshopBatchUpdateItem[]): Promise<Array<Record<string, any>>> => {
            return ApiService.patch<Array<Record<string, any>>, NuvemshopBatchUpdateItem[]>(
                "/nuvemshop/products/stock-price",
                batchData,
                getRequestConfig()
            );
        },

        // DELETE /api/nuvemshop/products/{product_id}
        deleteProduct: async (productId: number): Promise<void> => {
            return ApiService.delete<void>(
                `/nuvemshop/products/${productId}`,
                getRequestConfig()
            );
        }
    }
};