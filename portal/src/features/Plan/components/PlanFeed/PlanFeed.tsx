import { useEffect, useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Check, Star, ArrowLeft } from 'lucide-react';
import styles from './PlanFeed.module.scss';
import { usePublicPlans } from '../../hooks/usePublicPlans';
import { Card } from '@/components/Card/Card';

export interface PlanFeedProps {
  category: 'course' | 'ecommerce';
}

export const PlanFeed = ({ category }: PlanFeedProps) => {
  const { publicPlans, loading, fetchPublicPlans } = usePublicPlans();
  const navigate = useNavigate();
  const [, setSearchParams] = useSearchParams();

  useEffect(() => {
    fetchPublicPlans();
  }, [fetchPublicPlans]);

  const handleSelectPlan = (planId: string) => {
    navigate(`/payment/checkout/${planId}`);
  };

  const handleBack = () => {
    setSearchParams({}); // Limpa os parâmetros de busca
  };

  // Filtragem e ordenação em memória (performance)
  const sortedAndFilteredPlans = useMemo(() => {
    const filtered = publicPlans.filter(p => p.category === category);
    return filtered.sort((a, b) => a.frequency - b.frequency);
  }, [publicPlans, category]);

  // Se estiver carregando, retornamos apenas a div do container
  // O Layout global cuidará do resto da estrutura
  if (loading) {
    return (
      <div className={styles.container}>
        <div className={styles.loadingState}>Carregando planos...</div>
      </div>
    );
  }

  const title = category === 'course' 
    ? "Planos de Cursos Online" 
    : "Planos de Automação para E-commerce";

  return (
      // Apenas a div container, pois o <Outlet /> do router já está dentro do Layout principal
      <div className={styles.container}>
        
        <header className={styles.header}>
          <button 
            onClick={handleBack} 
            className={styles.backButton}
            style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem', background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-neutral-600)', marginBottom: '1rem' }}
          >
            <ArrowLeft size={16} /> Voltar para seleção de perfis
          </button>
          <h1>{title}</h1> 
          {/* Adicionei uma descrição opcional para melhorar o header visualmente */}
          <p>Escolha a melhor opção para o seu negócio</p>
        </header>

        <div className={styles.grid}>
          {sortedAndFilteredPlans.map((plan) => (
            <Card 
              key={plan.publicId} 
              className={`${styles.planCard} ${plan.isRecommended ? styles.recommended : ''}`}
              onClick={() => handleSelectPlan(plan.publicId)}
            >
              {plan.isRecommended && (
                <div className={styles.badge}>
                  <Star size={12} fill="white" /> Recomendado
                </div>
              )}

              <Card.Body title={plan.name || ''}>
                <div className={styles.priceWrapper}>
                  <span className={styles.currency}>R$</span>
                  {/* Tratamento para exibir apenas o valor numérico grande */}
                  <span className={styles.value}>
                    {(plan.priceDisplay || '').replace('R$', '').trim()}
                  </span>
                </div>
                <p className={styles.billingInfo}>{plan.billingInfo}</p>
                
                <div className={styles.divider} />

                <ul className={styles.featuresList}>
                  {plan.features.map((feature: string, idx: number) => (
                    <li key={idx}>
                      <Check size={16} className={styles.checkIcon} />
                      {feature}
                    </li>
                  ))}
                </ul>
              </Card.Body>

              <Card.Actions>
                <button className={styles.selectBtn}>
                  {plan.isRecommended ? 'Quero este' : 'Selecionar'}
                </button>
              </Card.Actions>
            </Card>
          ))}
        </div>
      </div>
  );
};
