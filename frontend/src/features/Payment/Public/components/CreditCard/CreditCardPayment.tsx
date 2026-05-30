// src/features/Payment/Public/components/CreditCard/CreditCardPayment.tsx
import React from 'react';
import styles from './CreditCardPayment.module.scss';
import { MercadoPagoBrick } from '.';
import { useCreditCardPayment } from '../../hooks/useCreditCardPayment';
import type { CreditCardMode } from '../../../shared';

interface CreditCardPaymentProps {
  planName: string;
  planId: string;
  amount: number;
  mode: CreditCardMode;
  preferenceId: string;
  onPaymentSuccess?: () => void;
}

export const CreditCardPayment: React.FC<CreditCardPaymentProps> = ({ 
  planName, 
  planId,
  amount, 
  mode,
  preferenceId,
  onPaymentSuccess
}) => {
  
  const { loading, error, handleCreditCardSubmit } = useCreditCardPayment({
    planId,
    planName,
    amount,
    mode,
    onSuccess: () => {
      if (onPaymentSuccess) onPaymentSuccess();
    }
  });

  if (loading) {
    return (
      <div className={styles.loadingState}>
        <div className={styles.spinner}></div>
        <h3>Processando...</h3>
        <p>Estamos validando seu pagamento junto à operadora.</p>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h3>
          <i className="fas fa-credit-card"></i> 
          {mode === 'subscription' ? ' Assinatura Recorrente' : ' Pagamento via Cartão'}
        </h3>
        <p>
          Plano selecionado: <strong>{planName}</strong> <br/>
          Valor: <strong>R$ {amount.toFixed(2)}</strong>
        </p>
      </div>

      {error && (
        <div className="alert alert-danger">
          <i className="fas fa-times-circle"></i> {error}
        </div>
      )}

      <div className={styles.brickWrapper}>
        <MercadoPagoBrick
          config={{
            mode: mode,
            amount: amount,
            preferenceId: preferenceId
          }}
          onSubmit={handleCreditCardSubmit}
          onError={(e: unknown) => console.error("Erro brick", e)}
        />
      </div>

      <div className={styles.secureBadge}>
        <i className="fas fa-lock"></i>
        <span>Pagamento 100% seguro processado pelo Mercado Pago.</span>
      </div>
    </div>
  );
};
