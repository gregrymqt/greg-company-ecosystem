import React from 'react';
import { useFreeSample } from '../hooks/useFreeSample';
import { useNavigate } from 'react-router-dom';
import styles from './FreeSamplePage.module.scss';
import { ComparisonPanel } from '../components/ComparisonPanel/ComparisonPanel';
import { ConversionCTA } from '../components/ConversionCTA/ConversionCTA';
import { ProcessingProgress } from '../components/ProcessingProgress/ProcessingProgress';
import { UrlInputForm } from '../components/UrlInputForm/UrlInputForm';

export const FreeSamplePage: React.FC = () => {
    const {
        products,
        isProcessing,
        globalError,
        startDemoProcess,
        resetDemoState
    } = useFreeSample();

    // Verifica se houve erro de limite pelo status ou mensagem
    const limitKeywords = ['limite', 'limit', '429'];
    const isLimitReached =
        limitKeywords.some(keyword => globalError?.toLowerCase().includes(keyword)) ||
        products.some(p => p.status === 'failed' && limitKeywords.some(keyword => p.error?.toLowerCase().includes(keyword)));

    const navigate = useNavigate();

    return (
        <main className={styles.pageContainer}>
            <section className={styles.content}>
                {/* 1. Formulário de Entrada: Só aparece se não houver produtos processando ou se resetarmos */}
                {products.length === 0 && (
                    <UrlInputForm onSubmit={startDemoProcess} isLoading={isProcessing} />
                )}

                {/* 2. Feedback em Tempo Real: Aparece durante o processamento do stream */}
                {(isProcessing || (products.length > 0 && !isLimitReached)) && (
                    <ProcessingProgress products={products} />
                )}

                {/* 3. O Clímax: Painel de Comparação aparece para os itens concluídos */}
                <ComparisonPanel products={products} />

                {/* 4. Gatilhos de Conversão (CTA): 
                    Aparece se terminar com sucesso ou se o Rate Limiter barrar o usuário 
                */}
                {(isLimitReached) && (
                    <ConversionCTA
                        reason="limit_reached"
                        onUpgrade={() => navigate('/plans?type=ecommerce')}
                    />
                )}

                {products.some(p => p.status === 'completed') && !isProcessing && (
                    <div className={styles.actionsFooter}>
                        <button className={styles.resetBtn} onClick={resetDemoState}>
                            <i className="fas fa-sync" /> Testar outros links
                        </button>
                        <ConversionCTA
                            reason="success"
                            onUpgrade={() => navigate('/plans?type=ecommerce')}
                        />
                    </div>
                )}
            </section>
        </main>
    );
};