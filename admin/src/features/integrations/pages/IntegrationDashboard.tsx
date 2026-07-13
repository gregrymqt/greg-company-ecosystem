import React, { useState } from 'react';
import { useIntegrations, type IntegrationProvider } from '../hooks/useIntegrations';
import { AIScraperPanel } from '../components/AIScraperPanel/AIScraperPanel';
import styles from './IntegrationDashboard.module.scss';

export const IntegrationDashboard: React.FC = () => {
  // Estado local para a Aba ativa
  const [activeTab, setActiveTab] = useState<IntegrationProvider | 'ai'>('shopify');

  // Hook da lógica de negócios, repassando um provedor de loja apenas quando não for a aba de IA
  const storeProvider = activeTab === 'ai' ? 'shopify' : activeTab;
  const { fallbackInfo, clearFallback, isLoading } = useIntegrations(storeProvider);

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

          {/* Espaço reservado para a futura Tabela de Produtos */}
          <div className={styles.tableArea}>
            {/* <ProductTable provider={activeTab} /> */}
            <p>A tabela de produtos para {activeTab === 'shopify' ? 'Shopify' : 'Nuvemshop'} será renderizada aqui.</p>
          </div>
        </>
      )}
    </div>
  );
};
