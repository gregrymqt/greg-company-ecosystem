// src/features/Payment/Public/components/Pix/PixPayment.tsx
import React from 'react';

import styles from '../../styles/PixPayment.module.scss';
import { usePixPayment } from '../../hooks/usePixPayment';
import { PixForm, PixQRCode } from '.';

interface PixPaymentProps {
  amount: number;
  planId: string;
  planName: string;
  userParams?: { name: string; email: string };
  onPaymentSuccess?: () => void;
}

export const PixPayment: React.FC<PixPaymentProps> = ({ 
  amount,
  planId,
  userParams, 
  onPaymentSuccess 
}) => {
  
  const { 
    step, 
    loading, 
    pixData, 
    docTypes, 
    handleCreatePix, 
    copyPixCode,
    error
  } = usePixPayment({
    planId,
    amount,
    onSuccess: () => {
      if (onPaymentSuccess) onPaymentSuccess();
    }
  });

  // Tela de Sucesso (Controlada pelo WebSocket agora)
  // Como o PaymentLayout gerencia o sucesso global, apenas retornamos null
  if (step === 'SUCCESS') {
    return null;
  }

  return (
    <div className={styles.pixContainer}>
      
      {error && (
        <div className="alert alert-danger mb-3">
          <i className="fas fa-exclamation-triangle"></i> {error}
        </div>
      )}

      {step === 'FORM' && (
        <>
          <p className={styles.description}>
            Preencha os dados para gerar o QR Code. O pagamento é confirmado automaticamente em alguns segundos.
          </p>
          <PixForm 
            onSubmit={handleCreatePix} 
            isLoading={loading}
            docTypes={docTypes}
            defaultEmail={userParams?.email}
            defaultName={userParams?.name}
          />
        </>
      )}

      {step === 'QR_CODE' && pixData && (
        <PixQRCode 
          data={pixData} 
          onCopy={copyPixCode} 
        />
      )}
    </div>
  );
};
