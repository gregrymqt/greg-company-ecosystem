/**
 * Analytics Feature - AnalyticsPage
 * Página principal do dashboard de analytics
 * Utiliza componentes genéricos e mobile-first design
 */

import { useState } from 'react';
import { AnalyticsCarousel } from '@/features/analytics/components/AnalyticsCarousel';
import { useAnalytics } from '@/features/analytics/hooks/useAnalytics';
import type { AnalyticsFilters } from '@/features/analytics/types/analytics.types';
import styles from './AnalyticsPage.module.scss';
import { RefreshCw, Download, Filter } from 'lucide-react';

export const AnalyticsPage = () => {
  const {
    dashboard,
    products,
    isLoading,
    error,
    filters,
    syncData,
    exportToExcel,
    updateFilters,
  } = useAnalytics();

  const [showFilters, setShowFilters] = useState(false);
  const [localFilters, setLocalFilters] = useState<AnalyticsFilters>(filters);

  // Handler para aplicar filtros
  const handleApplyFilters = () => {
    updateFilters(localFilters);
    setShowFilters(false);
  };

  // Handler para limpar filtros
  const handleClearFilters = () => {
    const emptyFilters: AnalyticsFilters = {};
    setLocalFilters(emptyFilters);
    updateFilters(emptyFilters);
  };

  if (isLoading && !dashboard) {
    return (
      <div className={styles.analyticsPage}>
        <div className={styles.loading}>
          <RefreshCw className="animate-spin" size={32} />
          <p>Carregando analytics...</p>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.analyticsPage}>
      {/* Header */}
      <header className={styles.pageHeader}>
        <h1 className={styles.pageTitle}>Analytics Dashboard</h1>
        <p className={styles.pageSubtitle}>
          Monitore métricas de produtos e estoque em tempo real
        </p>
      </header>

      {/* Error State */}
      {error && (
        <div className={styles.error}>
          <p>{error}</p>
        </div>
      )}

      {/* Dashboard Cards */}
      {dashboard && (
        <div className={styles.dashboardGrid}>
          <div className={styles.dashboardCard}>
            <h3 className={styles.cardTitle}>Total de Produtos</h3>
            <p className={styles.cardValue}>{dashboard.totalProducts}</p>
            <p className={styles.cardSubtext}>Produtos cadastrados</p>
          </div>

          <div className={styles.dashboardCard}>
            <h3 className={styles.cardTitle}>Produtos Críticos</h3>
            <p className={`${styles.cardValue} ${styles.danger}`}>
              {dashboard.criticalProducts}
            </p>
            <p className={styles.cardSubtext}>Estoque baixo ou esgotado</p>
          </div>

          <div className={styles.dashboardCard}>
            <h3 className={styles.cardTitle}>Estoque Médio</h3>
            <p className={`${styles.cardValue} ${styles.success}`}>
              {dashboard.averageStock.toFixed(0)}
            </p>
            <p className={styles.cardSubtext}>Unidades por produto</p>
          </div>

          <div className={styles.dashboardCard}>
            <h3 className={styles.cardTitle}>Receita Total</h3>
            <p className={`${styles.cardValue} ${styles.success}`}>
              {new Intl.NumberFormat('pt-BR', {
                style: 'currency',
                currency: 'BRL',
                minimumFractionDigits: 0,
              }).format(dashboard.totalRevenue)}
            </p>
            <p className={styles.cardSubtext}>Valor total em estoque</p>
          </div>

          <div className={styles.dashboardCard}>
            <h3 className={styles.cardTitle}>Produtos Esgotados</h3>
            <p className={`${styles.cardValue} ${styles.warning}`}>
              {dashboard.outOfStockProducts}
            </p>
            <p className={styles.cardSubtext}>Necessitam reposição urgente</p>
          </div>

          <div className={styles.dashboardCard}>
            <h3 className={styles.cardTitle}>Última Sincronização</h3>
            <p className={styles.cardValue} style={{ fontSize: '1.25rem' }}>
              {new Date(dashboard.lastSync).toLocaleString('pt-BR')}
            </p>
            <p className={styles.cardSubtext}>Dados atualizados</p>
          </div>
        </div>
      )}

      {/* Actions */}
      <div className={styles.actions}>
        <button 
          className={`${styles.btn} ${styles.primary}`}
          onClick={syncData}
          disabled={isLoading}
        >
          <RefreshCw size={16} className={isLoading ? 'animate-spin' : ''} />
          Sincronizar Dados
        </button>

        <button 
          className={`${styles.btn} ${styles.success}`}
          onClick={exportToExcel}
          disabled={isLoading}
        >
          <Download size={16} />
          Exportar Excel
        </button>

        <button 
          className={`${styles.btn} ${styles.secondary}`}
          onClick={() => setShowFilters(!showFilters)}
        >
          <Filter size={16} />
          {showFilters ? 'Ocultar Filtros' : 'Mostrar Filtros'}
        </button>
      </div>

      {/* Filters Section */}
      {showFilters && (
        <div className={styles.filterSection}>
          <h3 className={styles.filterTitle}>Filtros de Busca</h3>
          
          <div className={styles.filterGrid}>
            <select
              className={styles.filterInput}
              value={localFilters.status || ''}
              onChange={(e) => setLocalFilters({
                ...localFilters,
                status: e.target.value as any || undefined,
              })}
            >
              <option value="">Todos os Status</option>
              <option value="OK">OK</option>
              <option value="CRITICO">Crítico</option>
              <option value="ESGOTADO">Esgotado</option>
              <option value="REPOR">Repor</option>
            </select>

            <input
              type="text"
              className={styles.filterInput}
              placeholder="Categoria"
              value={localFilters.category || ''}
              onChange={(e) => setLocalFilters({
                ...localFilters,
                category: e.target.value || undefined,
              })}
            />

            <input
              type="number"
              className={styles.filterInput}
              placeholder="Estoque Mínimo"
              value={localFilters.minStock || ''}
              onChange={(e) => setLocalFilters({
                ...localFilters,
                minStock: e.target.value ? Number(e.target.value) : undefined,
              })}
            />

            <input
              type="number"
              className={styles.filterInput}
              placeholder="Estoque Máximo"
              value={localFilters.maxStock || ''}
              onChange={(e) => setLocalFilters({
                ...localFilters,
                maxStock: e.target.value ? Number(e.target.value) : undefined,
              })}
            />
          </div>

          <div className={styles.actions} style={{ marginTop: '1rem' }}>
            <button 
              className={`${styles.btn} ${styles.primary}`}
              onClick={handleApplyFilters}
            >
              Aplicar Filtros
            </button>
            
            <button 
              className={`${styles.btn} ${styles.secondary}`}
              onClick={handleClearFilters}
            >
              Limpar Filtros
            </button>
          </div>
        </div>
      )}

      {/* Carousel */}
      <AnalyticsCarousel 
        products={products}
        title="Produtos em Destaque"
        maxItems={12}
        autoplay={true}
        onProductClick={(product) => {
          console.log('Produto clicado:', product);
          // Pode abrir um modal com detalhes, navegar para outra página, etc.
        }}
      />
    </div>
  );
};
