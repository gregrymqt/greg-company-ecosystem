import styles from './ProcessingProgress.module.scss';
import type { DemoProductItem, ProcessStatus } from '../../types/free-sample.types';

interface ProcessingProgressProps {
  /** Array de produtos/URLs sendo processados pelo hook useFreeSample */
  products: DemoProductItem[];
}

/**
 * Helper para traduzir o status técnico do microserviço Python 
 * em uma cópia persuasiva e dinâmica na tela do usuário.
 */
const getStatusMetadata = (status: ProcessStatus, error?: string) => {
  switch (status) {
    case 'pending':
      return { text: 'Entrando na esteira de aceleração...', class: styles.pending };
    case 'scraping':
      return { text: 'Rastejando e limpando o HTML do fornecedor...', class: styles.scraping };
    case 'generating':
      return { text: 'IA injetando gatilhos mentais e tags de SEO agressivas...', class: styles.generating };
    case 'completed':
      return { text: 'Página de vendas otimizada com sucesso! 🔥', class: styles.completed };
    case 'failed':
      return { text: error || 'Bloqueio de segurança do fornecedor detectado.', class: styles.failed };
    default:
      return { text: 'Aguardando inicialização...', class: styles.idle };
  }
};

/**
 * Limpa e encurta a URL exibida para não quebrar a diagramação do grid de progresso
 */
const formatUrlDisplay = (url: string): string => {
  try {
    const parsed = new URL(url);
    return `${parsed.hostname}${parsed.pathname.length > 20 ? parsed.pathname.substring(0, 20) + '...' : parsed.pathname}`;
  } catch {
    return url.length > 35 ? url.substring(0, 35) + '...' : url;
  }
};

export function ProcessingProgress({ products }: ProcessingProgressProps) {
  if (products.length === 0) return null;

  return (
    <div className={styles.wrapper}>
      <h3 className={styles.title}>Esteira de Processamento em Tempo Real</h3>
      
      <div className={styles.list}>
        {products.map((product, index) => {
          const meta = getStatusMetadata(product.status, product.error);
          const isDone = product.status === 'completed';
          const isFailed = product.status === 'failed';

          return (
            <div key={index} className={`${styles.card} ${meta.class}`}>
              <div className={styles.cardHeader}>
                <div className={styles.urlBlock}>
                  <i className={`fas ${isDone ? 'fa-check-circle' : isFailed ? 'fa-exclamation-triangle' : 'fa-link'}`} />
                  <span className={styles.urlText} title={product.url}>
                    {product.enhanced?.seoTitle || product.original?.title || formatUrlDisplay(product.url)}
                  </span>
                </div>
                <div className={styles.percentage}>
                  {product.progress}%
                </div>
              </div>

              <div className={styles.statusMessage}>
                {meta.text}
              </div>

              <div className={styles.progressTrack}>
                <div 
                  className={styles.progressBar} 
                  style={{ width: `${product.progress}%` }}
                />
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}