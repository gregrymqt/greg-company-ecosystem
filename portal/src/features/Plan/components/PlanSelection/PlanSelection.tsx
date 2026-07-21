import React from 'react';
import { GraduationCap, Bot } from 'lucide-react';
import { Card } from '@/components/Card/Card';
import styles from './PlanSelection.module.scss';

interface PlanSelectionProps {
  onSelect: (category: 'course' | 'ecommerce') => void;
}

export const PlanSelection: React.FC<PlanSelectionProps> = ({ onSelect }) => {

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <h1 className={styles.title}>O que você está buscando?</h1>
        <p className={styles.subtitle}>
          Selecione a área que melhor atende ao seu momento e explore nossos planos
        </p>
      </header>

      <div className={styles.grid}>
        {/* Card de Cursos */}
        <Card className={styles.selectionCard}>
          <div className={styles.cardContent}>
            <div className={styles.iconWrapper}>
              <GraduationCap className={styles.icon} />
            </div>
            <h2 className={styles.cardTitle}>Área de Cursos</h2>
            <p className={styles.cardDescription}>
              Acesse conteúdos educacionais exclusivos, trilhas completas e impulsione o seu aprendizado com a nossa metodologia de ensino aprovada por milhares de alunos.
            </p>
          </div>
          
          <div className={styles.actions}>
            <button 
              className={`${styles.button} ${styles.primaryBtn}`}
              onClick={() => onSelect('course')}
            >
              Ver Planos de Cursos
            </button>
          </div>
        </Card>

        {/* Card de E-commerce */}
        <Card className={styles.selectionCard}>
          <div className={styles.cardContent}>
            <div className={styles.iconWrapper}>
              <Bot className={styles.icon} />
            </div>
            <h2 className={styles.cardTitle}>Automação com IA</h2>
            <p className={styles.cardDescription}>
              Maximize as vendas do seu negócio com ferramentas avançadas de scraping e inteligência artificial aplicadas para a escalabilidade de lojistas modernos.
            </p>
          </div>
          
          <div className={styles.actions}>
            <button 
              className={`${styles.button} ${styles.primaryBtn}`}
              onClick={() => onSelect('ecommerce')}
            >
              Ver Planos de Automação
            </button>
          </div>
        </Card>
      </div>
    </div>
  );
};
