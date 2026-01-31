/**
 * Analytics Feature - ProductCard Component
 * Componente de card individual para exibição de métricas de produto
 * Utiliza o componente genérico Card
 */

import { Card } from '@/components/Card/Card';
import type { ProductMetric } from '@/features/analytics/types/analytics.types';
import { PRODUCT_STATUS } from '@/features/analytics/types/analytics.types';
import styles from '@/features/analytics/styles/ProductCard.module.scss';

interface ProductCardProps {
  product: ProductMetric;
  onClick?: (product: ProductMetric) => void;
}

export const ProductCard = ({ product, onClick }: ProductCardProps) => {
  
  // Define a classe CSS baseada no status
  const getStatusClass = (status: string) => {
    switch (status) {
      case PRODUCT_STATUS.CRITICO:
        return styles.critical;
      case PRODUCT_STATUS.ESGOTADO:
        return styles.outOfStock;
      case PRODUCT_STATUS.REPOR:
        return styles.refill;
      default:
        return styles.ok;
    }
  };

  return (
    <Card 
      data={product} 
      onClick={onClick}
      className={styles.productCard}
    >
      {product.thumbnail && (
        <Card.Image 
          src={product.thumbnail} 
          alt={product.name}
          badge={product.status}
        />
      )}
      
      <Card.Body title={product.name}>
        <div className={styles.productInfo}>
          <div className={styles.infoRow}>
            <span className={styles.label}>Categoria:</span>
            <span className={styles.value}>{product.category}</span>
          </div>
          
          <div className={styles.infoRow}>
            <span className={styles.label}>Estoque:</span>
            <span className={`${styles.value} ${getStatusClass(product.status)}`}>
              {product.stock} unidades
            </span>
          </div>
          
          <div className={styles.infoRow}>
            <span className={styles.label}>Preço:</span>
            <span className={styles.value}>
              {new Intl.NumberFormat('pt-BR', {
                style: 'currency',
                currency: 'BRL',
              }).format(product.price)}
            </span>
          </div>
          
          <div className={styles.infoRow}>
            <span className={styles.label}>Status:</span>
            <span className={`${styles.badge} ${getStatusClass(product.status)}`}>
              {product.status}
            </span>
          </div>
        </div>
      </Card.Body>
      
      <Card.Actions>
        <span className={styles.lastUpdate}>
          Atualizado em: {new Date(product.lastUpdate).toLocaleDateString('pt-BR')}
        </span>
      </Card.Actions>
    </Card>
  );
};
