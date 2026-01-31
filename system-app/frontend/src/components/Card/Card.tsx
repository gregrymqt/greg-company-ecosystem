import { type ReactNode } from 'react';
import styles from './Card.module.scss';

// --- Interfaces dos Sub-componentes ---

interface CardImageProps {
  src: string;
  alt: string;
  badge?: string; // Opcional: Para colocar etiquetas como "Novo", "Promoção"
}

interface CardBodyProps {
  title: string;
  children: ReactNode; // Descrição ou conteúdo livre
}

interface CardActionsProps {
  children: ReactNode; // Botões
}

// --- Interface do Componente Principal ---

interface CardProps<T> {
  data?: T; // O Objeto Genérico (opcional se for apenas visual)
  children: ReactNode;
  className?: string;
  onClick?: (item: T) => void; // Evento de clique retornando o objeto tipado
}

// --- Componente Raiz ---
export const Card = <T,>({ 
  data, 
  children, 
  className = '', 
  onClick 
}: CardProps<T>) => {
  
  const handleClick = () => {
    if (onClick && data) {
      onClick(data);
    }
  };

  return (
    <article 
      className={`${styles.card} ${className}`} 
      onClick={handleClick}
      // Se tiver onClick, adiciona cursor pointer
      style={onClick ? { cursor: 'pointer' } : undefined}
    >
      {children}
    </article>
  );
};

// --- Sub-componentes (Attached Properties) ---

const Image = ({ src, alt, badge }: CardImageProps) => (
  <div className={styles.imageWrapper}>
    <img src={src} alt={alt} loading="lazy" />
    {badge && <span className={styles.badge}>{badge}</span>}
  </div>
);

const Body = ({ title, children }: CardBodyProps) => (
  <div className={styles.body}>
    <h3>{title}</h3>
    <div className={styles.content}>{children}</div>
  </div>
);

const Actions = ({ children }: CardActionsProps) => (
  <div className={styles.actions}>
    {children}
  </div>
);

// Acoplando os sub-componentes ao Pai
Card.Image = Image;
Card.Body = Body;
Card.Actions = Actions;