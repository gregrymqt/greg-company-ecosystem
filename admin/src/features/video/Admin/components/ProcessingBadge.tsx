import React, { useEffect } from 'react';
import { useVideoProgress } from '../hooks/useVideoProgress';
import styles from '../styles/ProcessingBadge.module.scss';

interface Props {
  storageIdentifier: string;
  status: number;
  onProcessComplete: () => void;
}

export const ProcessingBadge: React.FC<Props> = ({ storageIdentifier, status, onProcessComplete }) => {
  const { progress, status: wsStatus, error } = useVideoProgress(storageIdentifier);

  useEffect(() => {
    if (wsStatus === "SUCCESS") {
      onProcessComplete();
    }
  }, [wsStatus, onProcessComplete]);

  if (wsStatus === "IDLE" || wsStatus === "SUCCESS") {
    // Retorna null quando o processamento terminou ou antes de conectar
    // para que a listagem controle a renderização com o status final.
    return null; 
  }

  return (
    <div className={styles.badgeContainer}>
      <div className={styles.statusRow}>
        <span style={{ color: wsStatus === "FAILED" ? "red" : "inherit" }}>
          {wsStatus === "FAILED" ? (error || 'Erro no processamento') : 'Processando...'}
        </span>
        <strong>{progress}%</strong>
      </div>
      
      {/* Barra de Progresso Simples */}
      <div className={styles.progressTrack}>
        <div 
          className={styles.progressBar}
          style={{ 
            width: `${progress}%`,
            backgroundColor: wsStatus === "FAILED" ? "red" : undefined
          }} 
        />
      </div>
    </div>
  );
};
