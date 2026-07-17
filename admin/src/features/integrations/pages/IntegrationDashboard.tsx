import React, { useState, useEffect } from 'react';
import { useIntegrations, type IntegrationProvider } from '../hooks/useIntegrations';
import { AIScraperPanel } from '../components/AIScraperPanel/AIScraperPanel';
import { SyncProductTable } from '../components/SyncProductTable/SyncProductTable';
import { useAuth } from '@/features/auth/hooks/useAuth';
import { StorageService, STORAGE_KEYS } from '@/shared/services/storage.service';
import type { Product } from '../types/shared.types';
import styles from './IntegrationDashboard.module.scss';

export const IntegrationDashboard: React.FC = () => {
  // Estado local para a Aba ativa
  const [activeTab, setActiveTab] = useState<IntegrationProvider | 'ai'>('shopify');

  // Hook da lógica de negócios, repassando um provedor de loja apenas quando não for a aba de IA
  const storeProvider = activeTab === 'ai' ? 'shopify' : activeTab;
  const { 
    products, 
    pageInfo, 
    fetchProducts, 
    isLoading, 
    fallbackInfo, 
    clearFallback, 
    syncProduct, 
    updateProduct, 
    deleteProduct 
  } = useIntegrations(storeProvider);

  const { user } = useAuth();
  const tenantId = StorageService.getItem<string>(STORAGE_KEYS.TENANT_ID) || 
                   user?.tenant_id || 
                   (user as any)?.tenantId || 
                   user?.tenants?.[0] || 
                   '';

  // Carrega produtos ao alternar de aba
  useEffect(() => {
    if (activeTab !== 'ai') {
      fetchProducts(10);
    }
  }, [activeTab, fetchProducts]);

  // Helpers para mapear os dados internos do produto para os payloads de integração
  const mapProductToNuvemshop = (product: Product, tId: string) => ({
    tenant_id: tId,
    handle: { pt: product.sku },
    name: { pt: product.title || '' },
    description: { pt: product.description || '' },
    published: true,
    free_shipping: false,
    requires_shipping: true,
    categories: [],
    variants: [
      {
        price: product.price,
        sku: product.sku,
        stock: 999
      }
    ],
    images: product.images ? product.images.map((img: string) => ({ src: img })) : []
  });

  const mapProductToShopify = (product: Product, tId: string) => ({
    tenant_id: tId,
    title: product.title || '',
    descriptionHtml: product.description || '',
    vendor: 'Default Vendor',
    status: 'DRAFT' as const,
    variants: [
      {
        price: String(product.price || 0),
        sku: product.sku,
        optionValues: []
      }
    ],
    files: product.images ? product.images.map((img: string) => ({ originalSource: img, contentType: 'IMAGE' as const })) : []
  });

  const handleSync = async (product: Product) => {
    if (activeTab === 'shopify') {
      const payload = mapProductToShopify(product, tenantId);
      await syncProduct(payload);
    } else if (activeTab === 'nuvemshop') {
      const payload = mapProductToNuvemshop(product, tenantId);
      await syncProduct(payload);
    }
  };

  const handleUpdate = async (productId: string | number, data: unknown) => {
    await updateProduct(productId, data as any);
  };

  const handleDelete = async (productId: string | number) => {
    await deleteProduct(productId);
  };

  return (
    <div className={styles.container}>
      {/* Header */}
      <header className={styles.header}>
        <h1>Dashboard de Integrações</h1>
        <p>Gerencie produtos do e-commerce sincronizados com a plataforma.</p>
      </header>

      {/* Chaveador de Loja / Tabs */}
      <div className={styles.tabs}>
        <button
          className={`${styles.tab} ${activeTab === 'shopify' ? styles.active : ''}`}
          onClick={() => setActiveTab('shopify')}
          disabled={activeTab !== 'ai' && isLoading}
        >
          <i className="fab fa-shopify"></i> Shopify
        </button>
        <button
          className={`${styles.tab} ${activeTab === 'nuvemshop' ? styles.active : ''}`}
          onClick={() => setActiveTab('nuvemshop')}
          disabled={activeTab !== 'ai' && isLoading}
        >
          <i className="fas fa-shopping-cart"></i> Nuvemshop
        </button>
        <button
          className={`${styles.tab} ${activeTab === 'ai' ? styles.active : ''}`}
          onClick={() => setActiveTab('ai')}
        >
          <i className="fas fa-robot"></i> Automação & IA
        </button>
      </div>

      {activeTab === 'ai' ? (
        <AIScraperPanel />
      ) : (
        <>
          {/* Banner de Fallback CSV (Trello Requirement HTTP 202) */}
          {fallbackInfo && (
            <div className={styles.fallbackBanner}>
              <div className={styles.bannerContent}>
                <i className="fas fa-exclamation-triangle"></i>
                <div>
                  <strong>Ação executada em modo Fallback (CSV)</strong>
                  <p>{fallbackInfo.message}</p>
                </div>
              </div>
              <div className={styles.bannerActions}>
                <a
                  href={fallbackInfo.downloadUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className={styles.downloadBtn}
                >
                  <i className="fas fa-download"></i> Baixar CSV
                </a>
                <button className={styles.closeBtn} onClick={clearFallback} aria-label="Fechar alerta">
                  &times;
                </button>
              </div>
            </div>
          )}

          {/* Tabela de Produtos integrada */}
          <div className={styles.tableArea}>
            <SyncProductTable 
              data={products}
              isLoading={isLoading}
              provider={activeTab}
              onSync={handleSync}
              onUpdate={handleUpdate}
              onDelete={handleDelete}
              hasNextPage={pageInfo.hasNextPage}
              onNextPage={() => fetchProducts(10, pageInfo.endCursor || undefined)}
            />
          </div>
        </>
      )}
    </div>
  );
};
