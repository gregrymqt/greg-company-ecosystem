import React, { useState } from 'react';
import { useIntegrations, type IntegrationProvider } from '../hooks/useIntegrations';
import styles from './IntegrationDashboard.module.scss';

export const IntegrationDashboard: React.FC = () => {
  // Estado local para o Provider ativo
  const [activeProvider, setActiveProvider] = useState<IntegrationProvider>('shopify');

  // Hook da lógica de negócios, consumindo as chamadas de serviços das lojas
  const { fallbackInfo, clearFallback, isLoading } = useIntegrations(activeProvider);

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
          className={`${styles.tab} ${activeProvider === 'shopify' ? styles.active : ''}`}
          onClick={() => setActiveProvider('shopify')}
          disabled={isLoading}
        >
          <i className="fab fa-shopify"></i> Shopify
        </button>
        <button
          className={`${styles.tab} ${activeProvider === 'nuvemshop' ? styles.active : ''}`}
          onClick={() => setActiveProvider('nuvemshop')}
          disabled={isLoading}
        >
          <i className="fas fa-shopping-cart"></i> Nuvemshop
        </button>
      </div>

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

      {/* Espaço reservado para a futura Tabela de Produtos */}
      <div className={styles.tableArea}>
        {/* <ProductTable provider={activeProvider} /> */}
        <p>A tabela de produtos para {activeProvider === 'shopify' ? 'Shopify' : 'Nuvemshop'} será renderizada aqui.</p>
      </div>
    </div>
  );
};
