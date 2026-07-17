import React, { useState } from 'react';
import styles from './ProcessingProgress.module.scss';
import type { DemoProductItem, ProcessStatus } from '../../types/free-sample.types';
import { AlertService } from '../../../../shared/services/alert.service';

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
      const isTimeout = error?.includes('Instabilidade na fila');
      const text = isTimeout 
        ? error 
        : 'Não conseguimos acessar esse produto automaticamente devido a bloqueios do site. Tente colar o texto do produto manualmente.';
      return { text, class: styles.failed };
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
  const [manualTexts, setManualTexts] = useState<Record<string, string>>({});

  if (products.length === 0) return null;

  const handleManualSubmit = async (url: string) => {
    const text = manualTexts[url] || '';
    if (!text.trim()) {
      AlertService.notify('Atenção', 'Por favor, insira o texto do produto.', 'warning');
      return;
    }

    await AlertService.error(
      'Recurso Premium',
      'A otimização manual de textos copiados está disponível apenas para assinantes da Greg Company. Crie sua conta ou assine um plano para obter acesso completo.'
    );
  };

  return (
    <div className={styles.wrapper}>
      <h3 className={styles.title}>Esteira de Processamento em Tempo Real</h3>
      
      <div className={styles.list}>
        {products.map((product, index) => {
          const meta = getStatusMetadata(product.status, product.error);
          const isDone = product.status === 'completed';
          const isFailed = product.status === 'failed';
          const isTimeout = product.error?.includes('Instabilidade na fila');

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

              {isFailed && !isTimeout && (
                <div className={styles.manualInputBlock}>
                  <textarea
                    className={styles.manualTextArea}
                    placeholder="Cole aqui o título, descrição ou textos do produto que deseja otimizar..."
                    value={manualTexts[product.url] || ''}
                    onChange={(e) => setManualTexts(prev => ({ ...prev, [product.url]: e.target.value }))}
                  />
                  <button 
                    type="button"
                    className={styles.manualSubmitBtn}
                    onClick={() => handleManualSubmit(product.url)}
                  >
                    <i className="fas fa-magic" /> Otimizar Texto Manualmente
                  </button>
                </div>
              )}

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