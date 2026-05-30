/**
 * Analytics Feature - AnalyticsCarousel Component
 * Carrossel de produtos utilizando o componente genérico Carousel
 * Mobile-First com Swiper
 */

import { Carousel } from '@/components/Carousel/Carousel';
import { ProductCard } from './ProductCard';
import { useAnalyticsCarousel } from '@/features/analytics/hooks/useAnalyticsCarousel';
import type { ProductMetric } from '@/features/analytics/types/analytics.types';
import type { SwiperOptions } from 'swiper/types';
import styles from '@/features/analytics/styles/AnalyticsCarousel.module.scss';

interface AnalyticsCarouselProps {
  products: ProductMetric[];
  onProductClick?: (product: ProductMetric) => void;
  maxItems?: number;
  title?: string;
  autoplay?: boolean;
}

export const AnalyticsCarousel = ({ 
  products, 
  onProductClick,
  maxItems = 10,
  title = 'Produtos em Analytics',
  autoplay = true,
}: AnalyticsCarouselProps) => {
  
  const { carouselProducts, stats } = useAnalyticsCarousel({ products, maxItems });

  // Configuração customizada do Swiper para Analytics
  const swiperOptions: SwiperOptions = {
    spaceBetween: 20,
    slidesPerView: 1, // Mobile: 1 card
    autoplay: autoplay ? {
      delay: 4000,
      disableOnInteraction: false,
    } : false,
    breakpoints: {
      768: {
        slidesPerView: 2, // Tablet: 2 cards
        spaceBetween: 25,
      },
      992: {
        slidesPerView: 3, // Desktop: 3 cards
        spaceBetween: 30,
      },
    },
  };

  if (carouselProducts.length === 0) {
    return (
      <div className={styles.emptyState}>
        <p>Nenhum produto disponível para exibição.</p>
      </div>
    );
  }

  return (
    <section className={styles.analyticsCarousel}>
      <div className={styles.header}>
        <h2 className={styles.title}>{title}</h2>
        <div className={styles.stats}>
          <span className={styles.stat}>
            Total: <strong>{stats.total}</strong>
          </span>
          <span className={`${styles.stat} ${styles.critical}`}>
            Críticos: <strong>{stats.critical}</strong>
          </span>
          <span className={`${styles.stat} ${styles.healthy}`}>
            OK: <strong>{stats.healthy}</strong>
          </span>
        </div>
      </div>

      <Carousel
        items={carouselProducts}
        renderItem={(product) => (
          <ProductCard 
            product={product} 
            onClick={onProductClick}
          />
        )}
        keyExtractor={(product) => product.id}
        options={swiperOptions}
        className={styles.carousel}
      />
    </section>
  );
};
