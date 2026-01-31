import React from 'react';
import styles from '../styles/VideoMetadata.module.scss';
import { Card } from '@/components/Card/Card';
import type { PlayerVideoDto } from '../../shared';

interface VideoMetadataProps {
  data: PlayerVideoDto;
  onNext?: () => void;
  onPrev?: () => void;
}

export const VideoMetadata: React.FC<VideoMetadataProps> = ({ data, onNext, onPrev }) => {
  return (
    <div className={styles.metaContainer}>
      <Card<PlayerVideoDto> className={styles.customCard}>
        <Card.Body title={data.title}>
          <div className={styles.metaHeader}>
            {data.courseTitle && (
              <span className={styles.breadCrumb}>{data.courseTitle}</span>
            )}
            <span className={styles.durationBadge}>⏱ {data.durationFormatted}</span>
          </div>

          <p className={styles.description}>{data.description}</p>
        </Card.Body>

        <Card.Actions>
          <button
            onClick={onPrev}
            className={styles.actionBtn}
            disabled={!onPrev}
          >
            ← Anterior
          </button>
          <button
            onClick={onNext}
            className={`${styles.actionBtn} ${styles.primary}`}
            disabled={!onNext}
          >
            Próximo Aula →
          </button>
        </Card.Actions>
      </Card>
    </div>
  );
};
