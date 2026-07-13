import React from 'react';
import type { CourseRowUI, VideoCardUI } from '@/features/course/types/course.types';
import styles from './CourseDetailModal.module.scss';

interface CourseDetailModalProps {
  course: CourseRowUI;
  onClose: () => void;
  onVideoClick: (video: VideoCardUI) => void;
}

export const CourseDetailModal: React.FC<CourseDetailModalProps> = ({ course, onClose, onVideoClick }) => {
  return (
    <div className={styles.overlay} onClick={onClose}>
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <header className={styles.header}>
          <button className={styles.closeBtn} onClick={onClose} aria-label="Fechar">&times;</button>
          <h2 className={styles.title}>{course.categoryName}</h2>
          <div className={styles.meta}>
            <span className={styles.year}>{course.year || 'N/A'}</span>
            <span className={styles.creator}>{course.creator || 'Desconhecido'}</span>
          </div>
          <p className={styles.description}>
            {course.description || 'Nenhuma descrição disponível para este curso.'}
          </p>
        </header>
        
        <div className={styles.body}>
          <div className={styles.videoList}>
            {course.videos.map((video, index) => (
              <div 
                key={video.id} 
                className={styles.videoItem}
                onClick={() => onVideoClick(video)}
              >
                <img src={video.thumbnailUrl} alt={video.title} className={styles.videoThumbnail} />
                <div className={styles.videoInfo}>
                  <h4 className={styles.videoTitle}>{index + 1}. {video.title}</h4>
                  {video.description && <p className={styles.videoDesc}>{video.description}</p>}
                </div>
                <div className={styles.videoDuration}>{video.durationFormatted}</div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};
