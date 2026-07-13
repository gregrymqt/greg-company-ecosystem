import React from 'react';
import styles from '../styles/CourseRow.module.scss';
import { Carousel } from '@/components/Carousel/Carousel';
import type { CourseRowUI, VideoCardUI } from '@/features/course/types/course.types';
import { VideoCard } from './VideoCard';

interface CourseRowProps {
  data: CourseRowUI;
  onVideoClick: (video: VideoCardUI) => void;
  onInfoClick?: (course: CourseRowUI) => void;
}

export const CourseRow: React.FC<CourseRowProps> = ({ data, onVideoClick, onInfoClick }) => {
  
  // Se não houver vídeos, não renderiza a fileira
  if (!data.videos || data.videos.length === 0) return null;

  return (
    <section className={styles.rowContainer}>
      <div className={styles.header}>
        <h3 className={styles.rowTitle}>{data.categoryName}</h3>
        {onInfoClick && (
          <button 
            className={styles.infoButton} 
            onClick={() => onInfoClick(data)}
            title="Mais informações"
          >
            <span style={{ fontStyle: 'italic', fontWeight: 'bold' }}>i</span>
            Detalhes
          </button>
        )}
      </div>

      {data.lastWatchedVideo && (
        <div className={styles.continueWatchingBanner}>
          <div className={styles.bannerContent}>
            <span className={styles.bannerLabel}>Continuar Assistindo</span>
            <span className={styles.bannerTitle}>{data.lastWatchedVideo.title}</span>
          </div>
          <button 
            className={styles.playButton} 
            onClick={() => onVideoClick(data.lastWatchedVideo!)}
          >
            &#9654; Play
          </button>
        </div>
      )}
      
      <div className={styles.carouselWrapper}>
        <Carousel<VideoCardUI>
          items={data.videos}
          keyExtractor={(item) => item.id}
          // Renderização customizada usando nosso VideoCard específico
          renderItem={(video) => (
            <VideoCard 
              data={video} 
              onClick={onVideoClick} 
            />
          )}
          // Podemos sobrescrever opções do Swiper se necessário (opcional)
          options={{
             spaceBetween: 15,
             // Ajuste fino para o estilo Netflix (slides visíveis parcialmente)
             centeredSlides: false,
          }}
          className={styles.customCarousel}
        />
      </div>
    </section>
  );
};
