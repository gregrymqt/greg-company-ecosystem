import React, { useCallback, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './styles/CourseFeed.module.scss';
import { CourseRow } from '@/features/course/components/CourseRow';
import { CourseSkeleton } from '@/features/course/components/CourseSkeleton';
import { usePublicCourses } from '@/features/course/hooks/usePublicCourses';
import type { VideoCardUI, CourseRowUI } from '@/features/course/types/course.types';
import { CourseDetailModal } from '@/features/course/components/CourseDetailModal';

export const CourseFeed: React.FC = () => {
  const navigate = useNavigate();
  const [infoCourse, setInfoCourse] = useState<CourseRowUI | null>(null);

  // Consumindo o Hook refatorado
  const { 
    courses, 
    isLoading, 
    error, 
    refresh,
    sentinelRef 
  } = usePublicCourses(); 

  // 2. A única responsabilidade sobre o vídeo aqui é o redirecionamento
  const handleVideoClick = useCallback((video: VideoCardUI) => {
    // Apenas muda a rota. A feature 'Player' assumirá a partir daqui.
    navigate(`/player/${video.id}`);
  }, [navigate]);

  // Tratamento de Erro de Carga
  if (error && courses.length === 0) {
    return (
      <div className={styles.errorContainer}>
        <p>{error}</p>
        <button onClick={refresh} className={styles.retryBtn}>Tentar Novamente</button>
      </div>
    );
  }

  return (
    <main className={styles.feedContainer}>
      <h1 className={styles.pageTitle}>Catálogo de Cursos</h1>

      {/* Estado de Loading Inicial (Skeleton) */}
      {isLoading && courses.length === 0 ? (
        <CourseSkeleton />
      ) : (
        <div className={styles.rowsList}>
          {/* Renderiza as Categorias (Rows) */}
          {courses.map((category) => (
            <CourseRow 
              key={category.id} 
              data={category} 
              onVideoClick={handleVideoClick} 
              onInfoClick={setInfoCourse}
            />
          ))}
        </div>
      )}

      {/* Sentinela do Infinite Scroll 
          Quando essa div invisível entra na tela, o Hook carrega mais. */}
      <div ref={sentinelRef} className={styles.infiniteScrollTrigger}>
        {isLoading && courses.length > 0 && (
           <span className={styles.loadingMore}>Carregando mais cursos...</span>
        )}
      </div>

      {infoCourse && (
        <CourseDetailModal 
          course={infoCourse} 
          onClose={() => setInfoCourse(null)} 
          onVideoClick={handleVideoClick} 
        />
      )}
    </main>
  );
};
