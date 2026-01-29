import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Check, Star } from 'lucide-react';
import styles from '../styles/PublicPlansList.module.scss';
import { usePublicPlans } from '@/features/Plan/hooks/usePublicPlans';
import { Card } from '@/components/Card/Card'; // Ajuste o caminho se necessário

export const PlanFeed = () => {
  const { publicPlans, loading, fetchPublicPlans } = usePublicPlans();
  const navigate = useNavigate();

  useEffect(() => {
    fetchPublicPlans();
  }, [fetchPublicPlans]);

  const handleSelectPlan = (planId: string) => {
    navigate(`/payment/checkout/${planId}`);
  };

  // Se estiver carregando, retornamos apenas a div do container
  // O Layout global cuidará do resto da estrutura
  if (loading) {
    return (
      <div className={styles.container}>
        <div className={styles.loadingState}>Carregando planos...</div>
      </div>
    );
  }

  // Ordenação lógica (ex: Mensal -> Anual)
  const sortedPlans = [...publicPlans].sort((a, b) => a.frequency - b.frequency); 

  return (
      // Apenas a div container, pois o <Outlet /> do router já está dentro do Layout principal
      <div className={styles.container}>
        
        <header className={styles.header}>
          <h1>Planos Disponíveis</h1> 
          {/* Adicionei uma descrição opcional para melhorar o header visualmente */}
          <p>Escolha a melhor opção para o seu negócio</p>
        </header>

        <div className={styles.grid}>
          {sortedPlans.map((plan) => (
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

              <Card.Body title={plan.name}>
                <div className={styles.priceWrapper}>
                  <span className={styles.currency}>R$</span>
                  {/* Tratamento para exibir apenas o valor numérico grande */}
                  <span className={styles.value}>
                    {plan.priceDisplay.replace('R$', '').trim()}
                  </span>
                </div>
                <p className={styles.billingInfo}>{plan.billingInfo}</p>
                
                <div className={styles.divider} />

                <ul className={styles.featuresList}>
                  {plan.features.map((feature, idx) => (
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