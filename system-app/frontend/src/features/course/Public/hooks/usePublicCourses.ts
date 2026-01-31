/**
 * Hook para gerenciar cursos na área pública
 * Implementa infinite scroll para carregamento progressivo
 */

import { useState, useEffect, useRef, useCallback } from 'react';
import { publicCourseService } from '@/features/course/Public/services/course.service';
import type { CourseRowUI } from '@/features/course/shared/types/course.types';
import { ApiError } from '@/shared/services/api.service';

export const usePublicCourses = (pageSize: number = 5) => {
  const [courses, setCourses] = useState<CourseRowUI[]>([]);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Ref para o elemento sentinela do scroll infinito
  const sentinelRef = useRef<HTMLDivElement>(null);

  // Função que busca dados (memoizada)
  const fetchCourses = useCallback(async (pageNum: number, isRefresh = false) => {
    setIsLoading(true);
    setError(null);
    try {
      const { data, totalPages } = await publicCourseService.getAllPaginated(pageNum, pageSize);
      
      setCourses(prev => isRefresh ? data : [...prev, ...data]);
      setHasMore(pageNum < totalPages);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message || 'Erro ao carregar conteúdo.');
      } else {
        setError('Erro ao carregar conteúdo.');
      }
    } finally {
      setIsLoading(false);
    }
  }, [pageSize]);

  // Carga inicial
  useEffect(() => {
    fetchCourses(1, true);
  }, [fetchCourses]);

  // Lógica do Observer (Infinite Scroll)
  useEffect(() => {
    const element = sentinelRef.current;
    if (!element) return;

    const observer = new IntersectionObserver((entries) => {
      // Se a div sentinela apareceu E tem mais páginas E não está carregando
      if (entries[0].isIntersecting && hasMore && !isLoading) {
        const nextPage = page + 1;
        setPage(nextPage);
        fetchCourses(nextPage);
      }
    }, { threshold: 0.5 });

    observer.observe(element);
    return () => observer.disconnect();
  }, [hasMore, isLoading, page, fetchCourses]);

  return {
    courses,
    isLoading,
    error,
    refresh: () => fetchCourses(1, true),
    sentinelRef
  };
};
