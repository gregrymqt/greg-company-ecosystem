// src/features/Transactions/Public/hooks/useRefund.ts
import { useState } from 'react';
import { AlertService } from '@/shared/services/alert.service';
import { ApiError } from '@/shared/services/api.service';
import { UserTransactionsService } from '../services/userTransactions.service';

export const useRefund = () => {
    const [isProcessing, setIsProcessing] = useState(false);

    const requestRefund = async (paymentId: string) => {
        // 1. Confirmação
        const { isConfirmed } = await AlertService.confirm(
            'Solicitar Estorno?',
            'O processo será iniciado e você será notificado assim que concluir.'
        );

        if (!isConfirmed) return;

        try {
            setIsProcessing(true);

            // 2. Apenas envia o pedido HTTP
            await UserTransactionsService.requestRefund(paymentId);

            // Feedback imediato de que o pedido foi ENVIADO
            AlertService.notify(
                'Solicitação Enviada',
                'Aguarde a confirmação...',
                'info'
            );

        } catch (err) {
            const msg = err instanceof ApiError ? err.message : 'Erro ao solicitar.';
            AlertService.error('Erro', msg);
        } finally {
            setIsProcessing(false);
        }
    };

    return { requestRefund, isProcessing };
};
