// src/features/Transactions/Public/hooks/useRefundNotification.ts
import { useEffect, useState } from 'react';
import { AppHubsCSharp } from '@/shared/enums/hub/hub.enums';
import { useSocketListener } from '@/shared/hooks/useSocket';
import { AlertService } from '@/shared/services/alert.service';
import { socketService } from '@/shared/services/socket.service';
import type { RefundStatusData } from '../../shared';

export const useRefundNotification = (onSuccess?: () => void) => {
    const [refundStatus, setRefundStatus] = useState<'idle' | 'processing' | 'completed'>('idle');



    // 2. Ouve o evento "ReceiveRefundStatus"
    useSocketListener<RefundStatusData>(
        AppHubsCSharp.GlobalRealtime,
        'ReceiveRefundStatus',
        (data) => {
            console.log("Socket Refund:", data);

            if (data.status === 'completed') {
                setRefundStatus('completed');

                AlertService.notify(
                    'Reembolso Confirmado!',
                    'O valor foi estornado para o seu cartão.',
                    'success'
                );

                if (onSuccess) onSuccess();

            } else if (data.status === 'failed') {
                setRefundStatus('idle');
                AlertService.notify('Falha no Reembolso', data.message, 'error');
            }
        }
    );

    return { refundStatus, setRefundStatus };
};
