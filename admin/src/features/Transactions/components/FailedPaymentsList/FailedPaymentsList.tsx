// src/features/Transactions/Admin/components/FailedPaymentsList.tsx
import React, { useState, useEffect } from 'react';
import { useAdminTransactions } from '../../hooks/useAdminTransactions';
import type { PaymentItems } from '../../types/transactions.types';
import styles from './FailedPaymentsList.module.scss';
import { type TableColumn, Table } from '@/components/Table/Table';

export const FailedPaymentsList: React.FC = () => {
    // 1. Consumimos o hook real para buscar pagamentos
    const { transactions, loading, fetchTransactions } = useAdminTransactions();

    // 2. Estado para controlar a aba ativa ('rejected' ou 'refunded')
    const [activeTab, setActiveTab] = useState<'rejected' | 'refunded'>('rejected');

    // 3. Busca os dados ao mudar a aba
    useEffect(() => {
        // 'rejected' na UI mapeia para 'failed' no service/hook
        const filterType = activeTab === 'rejected' ? 'failed' : 'refunded';
        fetchTransactions(filterType);
    }, [activeTab, fetchTransactions]);

    // 4. Definição das colunas da Tabela
    const columns: TableColumn<PaymentItems>[] = [
        {
            header: 'Data',
            width: '20%',
            render: (item: PaymentItems) => new Date(item.createdAt).toLocaleDateString('pt-BR')
        },
        {
            header: 'Descrição / Motivo',
            width: '40%',
            render: (item: PaymentItems) => {
                const fallback = activeTab === 'rejected' ? 'Pagamento Recusado' : 'Reembolso Processado';
                const baseText = item.description || fallback;
                return item.paymentMethod ? `${baseText} (via ${item.paymentMethod})` : baseText;
            }
        },
        {
            header: 'Valor',
            width: '20%',
            render: (item: PaymentItems) =>
                new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(item.amount)
        },
        {
            header: 'Status',
            width: '20%',
            render: (item: PaymentItems) => {
                const statusMap: Record<string, string> = {
                    rejected: 'Recusado',
                    failed: 'Falhou',
                    cancelled: 'Cancelado',
                    refunded: 'Estornado',
                    partially_refunded: 'Estorno Parcial'
                };
                
                const normalizedStatus = item.status?.toLowerCase() || '';
                const label = statusMap[normalizedStatus] || item.status;

                return (
                    <span className={`${styles.statusBadge} ${styles[normalizedStatus] || ''}`}>
                        {label}
                    </span>
                );
            }
        }
    ];

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
                    data={transactions}
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
