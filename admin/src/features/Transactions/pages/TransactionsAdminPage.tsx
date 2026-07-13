import React from 'react';
import { FailedPaymentsList } from '../components/FailedPaymentsList/FailedPaymentsList';
import styles from '../styles/TransactionsAdminPage.module.scss';

export const TransactionsAdminPage: React.FC = () => {
    return (
        <main className={styles.pageContainer}>
            <header className={styles.pageHeader}>
                <h2>Administração de Transações</h2>
                <p>Monitore pagamentos recusados e estornos na plataforma.</p>
            </header>
            <section className={styles.contentArea}>
                <FailedPaymentsList />
            </section>
        </main>
    );
};
