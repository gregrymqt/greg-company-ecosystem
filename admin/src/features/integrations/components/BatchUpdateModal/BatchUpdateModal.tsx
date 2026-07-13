import React, { useState } from 'react';
import { Form } from '@/components/Form/Form';
import styles from './BatchUpdateModal.module.scss';

export interface SelectedVariant {
  sku: string;
  title?: string;
  variant_id?: number;
}

export interface BatchUpdateModalProps {
  isOpen: boolean;
  onClose: () => void;
  selectedVariants: SelectedVariant[];
  onSubmitBatch: (batchData: Array<Record<string, unknown>>) => Promise<void>;
}

export const BatchUpdateModal: React.FC<BatchUpdateModalProps> = ({
  isOpen,
  onClose,
  selectedVariants,
  onSubmitBatch
}) => {
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (!isOpen) return null;

  // Regra de Negócio: Nuvemshop API aceita no máx 50 por lote
  const isOverLimit = selectedVariants.length > 50;

  const handleSubmit = async (data: Record<string, unknown>) => {
    if (isOverLimit) return; // Segurança extra

    setIsSubmitting(true);
    try {
      // Monta o payload final conforme interface esperada
      const batchData = selectedVariants.map(variant => ({
        sku: variant.sku,
        variant_id: variant.variant_id,
        price: data.price !== '' && data.price !== null && data.price !== undefined ? Number(data.price) : undefined,
        stock: data.stock !== '' && data.stock !== null && data.stock !== undefined ? Number(data.stock) : undefined
      }));

      await onSubmitBatch(batchData);
      onClose(); // Fecha apenas após o sucesso
    } catch (error) {
      console.error("Erro ao enviar batch", error);
      // O erro visual já é tratado pelo callback/hook chamador usando AlertService
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className={styles.overlay}>
      <div className={styles.modal}>
        <header className={styles.header}>
          <h2>Atualização em Lote (Nuvemshop)</h2>
          <button 
            type="button" 
            className={styles.closeBtn} 
            onClick={onClose} 
            disabled={isSubmitting}
            aria-label="Fechar Modal"
          >
            &times;
          </button>
        </header>

        <div className={styles.content}>
          <p>
            Você selecionou <strong>{selectedVariants.length}</strong> variante(s).
          </p>

          {isOverLimit && (
            <div className={styles.alertError}>
              <i className="fas fa-exclamation-triangle"></i>
              A API da Nuvemshop aceita no máximo 50 variantes por lote. Por favor, reduza a seleção.
            </div>
          )}

          {/* Lista simplificada dos itens */}
          <div className={styles.variantList}>
            {selectedVariants.slice(0, 5).map((v, i) => (
              <div key={i} className={styles.variantItem}>
                <span className={styles.sku}>{v.sku}</span>
                <span className={styles.title}>{v.title || 'Sem título'}</span>
              </div>
            ))}
            {selectedVariants.length > 5 && (
              <div className={styles.moreItems}>... e mais {selectedVariants.length - 5} iten(s)</div>
            )}
          </div>

          {/* Formulário com Pattern de Composição */}
          <Form onSubmit={handleSubmit} defaultValues={{ price: '', stock: '' }}>
            <Form.Input 
              name="price" 
              label="Novo Preço Global (Opcional)" 
              type="number" 
            />
            <Form.Input 
              name="stock" 
              label="Novo Estoque Global (Opcional)" 
              type="number" 
            />

            <Form.Actions>
              <button 
                type="button" 
                className={styles.cancelBtn} 
                onClick={onClose} 
                disabled={isSubmitting}
              >
                Cancelar
              </button>
              
              {isOverLimit ? (
                <button type="button" disabled className={styles.cancelBtn} style={{ opacity: 0.5 }}>
                  Salvar Lote (Bloqueado)
                </button>
              ) : (
                <Form.Submit isLoading={isSubmitting}>
                  Salvar Lote
                </Form.Submit>
              )}
            </Form.Actions>
          </Form>
        </div>
      </div>
    </div>
  );
};
