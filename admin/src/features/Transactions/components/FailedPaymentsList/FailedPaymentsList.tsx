// src/features/Transactions/Admin/components/FailedPaymentsList.tsx
import React, { useState, useMemo } from 'react';
const usePaymentHistory = () => ({ payments: [], loading: false, error: null, refetch: () => {} }) as any;
import type { PaymentItems } from '../../types/transactions.types';
import styles from './FailedPaymentsList.module.scss';
import { type TableColumn, Table } from '@/components/Table/Table';

export const FailedPaymentsList: React.FC = () => {
    // 1. Consumimos o hook existente para buscar todos os pagamentos
    const { payments, loading, error, refetch } = usePaymentHistory();

    // 2. Estado para controlar a aba ativa ('rejected' ou 'refunded')
    const [activeTab, setActiveTab] = useState<'rejected' | 'refunded'>('rejected');

    // 3. Filtramos os dados com base na aba selecionada
    const filteredData = useMemo(() => {
        return payments.filter((item: any) => {
            const status = item.status?.toLowerCase() || '';

            if (activeTab === 'rejected') {
                // Considera recusados (rejected) ou cancelados (cancelled)
                return status === 'rejected' || status === 'cancelled';
            }
            if (activeTab === 'refunded') {
                // Considera estornados
                return status === 'refunded' || status === 'partially_refunded';
            }
            return false;
        });
    }, [payments, activeTab]);

    // 4. Definição das colunas da Tabela
    const columns: TableColumn<PaymentItems>[] = [
        {
            header: 'Data',
            width: '20%',
            render: (item: any) => new Date(item.createdAt).toLocaleDateString('pt-BR')
        },
        {
            header: 'Descrição / Motivo',
            width: '40%',
            render: () => (activeTab === 'rejected' ? 'Pagamento Recusado' : 'Reembolso Processado')
        },
        {
            header: 'Valor',
            width: '20%',
            render: (item: any) =>
                new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(item.amount)
        },
        {
            header: 'Status',
            width: '20%',
            render: (item: any) => (
                <span className={`${styles.statusBadge} ${styles[item.status?.toLowerCase()] || ''}`}>
                    {item.status === 'rejected' ? 'Recusado' : 'Estornado'}
                </span>
            )
        }
    ];

    if (error) {
        return (
            <div className={styles.errorContainer}>
                <p>Erro: {error}</p>
                <button onClick={refetch}>Tentar novamente</button>
            </div>
        );
    }

    return (
        <div className={styles.failedPaymentsContainer}>
            <h3>Pagamentos Recusados e Estornos</h3>

            {/* Navegação por Abas */}
            <div className={styles.tabsHeader}>
                <button
                    className={activeTab === 'rejected' ? styles.active : ''}
                    onClick={() => setActiveTab('rejected')}
                >
                    Recusados
                </button>
                <button
                    className={activeTab === 'refunded' ? styles.active : ''}
                    onClick={() => setActiveTab('refunded')}
                >
                    Estornados
                </button>
            </div>

            {/* Tabela Reutilizável */}
            <div className={styles.tabContent}>
                <Table<PaymentItems>
                    data={filteredData}
                    columns={columns}
                    isLoading={loading}
                    keyExtractor={(item: any) => item.id}
                    emptyMessage={
                        activeTab === 'rejected'
                            ? 'Nenhum pagamento recusado.'
                            : 'Nenhum estorno registrado.'
                    }
                />
            </div>
        </div>
    );
};
