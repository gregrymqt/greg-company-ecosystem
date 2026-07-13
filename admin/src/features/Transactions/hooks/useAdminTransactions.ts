import { useState, useCallback, useRef, useEffect } from 'react';
import { AdminTransactionsService } from '../services/adminTransactions.service';
import type { PaymentItems } from '../types/transactions.types';
import { AlertService } from '@/shared/services/alert.service';

export type TransactionFilterType = 'all' | 'failed' | 'refunded';

export const useAdminTransactions = () => {
  const [transactions, setTransactions] = useState<PaymentItems[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [currentFilter, setCurrentFilter] = useState<TransactionFilterType>('all');
  
  const loadingRef = useRef(false);
  const abortControllerRef = useRef<AbortController | null>(null);

  const fetchTransactions = useCallback(async (type?: TransactionFilterType) => {
    const filterToUse = type || currentFilter;
    
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      loadingRef.current = false;
    }
    
    abortControllerRef.current = new AbortController();

    if (loadingRef.current) return;

    loadingRef.current = true;
    setLoading(true);

    try {
      const statusMap: Record<TransactionFilterType, string | undefined> = {
        all: undefined,
        failed: 'rejected',
        refunded: 'refunded'
      };

      const data = await AdminTransactionsService.getAllTransactions({
        status: statusMap[filterToUse]
      });

      if (!abortControllerRef.current?.signal.aborted) {
        setTransactions(data);
        if (type) {
          setCurrentFilter(type);
        }
      }
    } catch (err) {
      if ((err as Error).name === 'AbortError' || (err as Error).name === 'CanceledError') {
        return; 
      }
      const errorMessage = (err as Error).message || 'Erro ao carregar transações.';
      AlertService.error('Erro', errorMessage);
    } finally {
      if (!abortControllerRef.current?.signal.aborted) {
        setLoading(false);
        loadingRef.current = false;
      }
    }
  }, [currentFilter]);

  // Limpeza no desmonte do componente
  useEffect(() => {
    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, []);

  return {
    transactions,
    loading,
    currentFilter,
    fetchTransactions
  };
};
