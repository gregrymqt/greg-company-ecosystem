/**
 * Analytics Feature - useAnalyticsCarousel Hook
 * Gerencia lógica específica do carrossel de analytics
 */

import { useMemo } from 'react';
import type { ProductMetric } from '@/features/analytics/types/analytics.types';

interface UseAnalyticsCarouselProps {
  products: ProductMetric[];
  maxItems?: number;
}

export const useAnalyticsCarousel = ({ products, maxItems = 10 }: UseAnalyticsCarouselProps) => {
  
  /**
   * Produtos filtrados e limitados para exibição no carrossel
   */
  const carouselProducts = useMemo(() => {
    return products.slice(0, maxItems);
  }, [products, maxItems]);

  /**
   * Produtos críticos (estoque baixo ou esgotado)
   */
  const criticalProducts = useMemo(() => {
    return products.filter(p => p.status === 'CRITICO' || p.status === 'ESGOTADO');
  }, [products]);

  /**
   * Produtos OK
   */
  const healthyProducts = useMemo(() => {
    return products.filter(p => p.status === 'OK');
  }, [products]);

  /**
   * Produtos que precisam reposição
   */
  const refilProducts = useMemo(() => {
    return products.filter(p => p.status === 'REPOR');
  }, [products]);

  /**
   * Agrupa produtos por status
   */
  const productsByStatus = useMemo(() => {
    return {
      OK: healthyProducts,
      CRITICO: criticalProducts.filter(p => p.status === 'CRITICO'),
      ESGOTADO: criticalProducts.filter(p => p.status === 'ESGOTADO'),
      REPOR: refilProducts,
    };
  }, [healthyProducts, criticalProducts, refilProducts]);

  /**
   * Estatísticas rápidas
   */
  const stats = useMemo(() => {
    return {
      total: products.length,
      critical: criticalProducts.length,
      healthy: healthyProducts.length,
      refill: refilProducts.length,
    };
  }, [products, criticalProducts, healthyProducts, refilProducts]);

  return {
    carouselProducts,
    criticalProducts,
    healthyProducts,
    refilProducts,
    productsByStatus,
    stats,
  };
};
