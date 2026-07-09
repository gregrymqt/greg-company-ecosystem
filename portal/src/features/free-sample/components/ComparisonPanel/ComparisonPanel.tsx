import { Card } from '../../../../components/Card/Card';
import styles from './ComparisonPanel.module.scss';
import type { DemoProductItem } from '../../types/free-sample.types';

interface ComparisonPanelProps {
  /** Array de produtos que vêm do estado do nosso useFreeSample hook */
  products: DemoProductItem[];
}

export function ComparisonPanel({ products }: ComparisonPanelProps) {
  // Filtra apenas os produtos que completaram com sucesso o pipeline do nosso bot Python
  const completedProducts = products.filter((p) => p.status === 'completed' && p.enhanced);

  if (completedProducts.length === 0) return null;

  return (
    <div className={styles.wrapper}>
      <header className={styles.sectionHeader}>
        <span className={styles.badgeLabel}>Resultado da Otimização</span>
        <h2>O Clímax da Conversão: Antes vs Depois</h2>
        <p>Veja a transformação imediata da estrutura de texto sem graça do fornecedor em uma máquina de vendas com IA.</p>
      </header>

      <div className={styles.comparisonGrid}>
        {completedProducts.map((product, index) => (
          <div key={index} className={styles.comparisonRow}>
            
            {/* LADO ESQUERDO: ANTES (DADOS CRUS DO FORNECEDOR) */}
            <div className={styles.column}>
              <div className={styles.columnTitle}>
                <i className="fas fa-history" /> Anúncio Original do Fornecedor
              </div>
              <Card className={styles.cardOriginal}>
                <Card.Image 
                  src={product.original?.imageUrl || 'https://placehold.co/600x400?text=Sem+Imagem'} 
                  alt={product.original?.title || 'Produto'} 
                  badge="Texto Cru"
                />
                <Card.Body title={product.original?.title || 'Sem Título Capturado'}>
                  <div className={styles.priceTag}>
                    {product.original?.price ? `Preço de Custo: ${product.original.price}` : 'Preço não informado'}
                  </div>
                  <p className={styles.descriptionText}>
                    {product.original?.description || 'Este fornecedor não incluiu descrição textual estruturada na landing page original.'}
                  </p>
                </Card.Body>
              </Card>
            </div>

            {/* DIVISOR VISUAL COM ÍCONE DE ACELERAÇÃO */}
            <div className={styles.dividerZone}>
              <div className={styles.lightningCircle}>
                <i className="fas fa-bolt" />
              </div>
            </div>

            {/* LADO DIREITO: DEPOIS (COPYWRITING AGRESSIVO + SEO DO NOSSO BOT) */}
            <div className={styles.column}>
              <div className={`${styles.columnTitle} ${styles.enhancedTitle}`}>
                <i className="fas fa-rocket" /> Copywriting Agressivo de Alta Conversão
              </div>
              <Card className={styles.cardEnhanced}>
                <Card.Image 
                  src={product.original?.imageUrl || 'https://placehold.co/600x400?text=Sem+Imagem'} 
                  alt={product.enhanced?.seoTitle || 'SEO'} 
                  badge="🔥 Gerado por IA"
                />
                <Card.Body title={product.enhanced?.seoTitle || 'Título SEO Indisponível'}>
                  <div className={styles.enhancedMeta}>
                    <span className={styles.seoBadge}>
                      <i className="fas fa-search" /> Meta Title Otimizado para o Google
                    </span>
                  </div>
                  
                  {/* Container que preserva as quebras de linha e parágrafos estruturados do LLMService */}
                  <div className={styles.copyContainer}>
                    {product.enhanced?.copywriting.split('\n\n').map((paragraph, pIdx) => (
                      <p key={pIdx}>{paragraph}</p>
                    ))}
                  </div>

                  <div className={styles.tagSection}>
                    <h4>Tags de Busca e Tráfego Pago:</h4>
                    <div className={styles.tagCloud}>
                      {product.enhanced?.tags.map((tag, tagIdx) => (
                        <span key={tagIdx} className={styles.tagBadge}>
                          #{tag.toLowerCase().replace(/\s+/g, '')}
                        </span>
                      ))}
                    </div>
                  </div>
                </Card.Body>
              </Card>
            </div>

          </div>
        ))}
      </div>
    </div>
  );
}