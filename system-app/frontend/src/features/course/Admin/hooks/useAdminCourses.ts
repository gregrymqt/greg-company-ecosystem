/**
 * Hook para gerenciar cursos na área administrativa
 * Lida com CRUD completo e paginação
 */

import { useState, useCallback } from 'react';
import { adminCourseService } from '@/features/course/Admin/services/course.service';
import { AlertService } from '@/shared/services/alert.service';
import { ApiError } from '@/shared/services/api.service';
import type { 
  CourseDto, 
  PaginatedResponse, 
  CreateCourseData, 
  UpdateCourseData 
} from '@/features/course/shared/types/course.types';

export const useAdminCourses = () => {
  const [loading, setLoading] = useState(false);
  const [coursesData, setCoursesData] = useState<PaginatedResponse<CourseDto> | null>(null);

  /**
   * Carrega cursos com paginação
   */
  const fetchCourses = useCallback(async (page = 1, pageSize = 10) => {
    setLoading(true);
    try {
      const response = await adminCourseService.getAll({ pageNumber: page, pageSize });
      setCoursesData(response);
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        AlertService.error('Erro', error.message);
      }
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Cria novo curso
   */
  const createCourse = async (data: CreateCourseData) => {
    setLoading(true);
    try {
      await adminCourseService.create(data);
      await AlertService.success('Sucesso!', 'Curso criado com sucesso.');
      return true;
    } catch (error) {
      const msg = error instanceof ApiError ? error.message : 'Erro ao criar curso.';
      AlertService.error('Erro', msg);
      return false;
    } finally {
      setLoading(false);
    }
  };

  /**
   * Atualiza curso existente
   */
  const updateCourse = async (id: string, data: UpdateCourseData) => {
    setLoading(true);
    try {
      await adminCourseService.update(id, data);
      await AlertService.success('Atualizado!', 'Dados do curso salvos.');
      return true;
    } catch (error) {
      const msg = error instanceof ApiError ? error.message : 'Erro ao atualizar curso.';
      AlertService.error('Erro', msg);
      return false;
    } finally {
      setLoading(false);
    }
  };

  /**
   * Deleta curso com confirmação e tratamento de conflitos
   */
  const deleteCourse = async (id: string) => {
    // Confirmação visual
    const { isConfirmed } = await AlertService.confirm(
      'Tem certeza?',
      'Esta ação removerá o curso permanentemente.'
    );

    if (!isConfirmed) return false;

    setLoading(true);
    try {
      await adminCourseService.delete(id);
      await AlertService.success('Deletado!', 'O curso foi removido.');

      // Atualiza lista local removendo o item deletado
      if (coursesData) {
        setCoursesData({
          ...coursesData,
          items: coursesData.items.filter(c => c.publicId !== id)
        });
      }
      return true;
    } catch (error) {
      // Tratamento especial para conflito (409) - curso tem vídeos
      if (error instanceof ApiError && error.status === 409) {
        AlertService.error(
          'Ação Bloqueada',
          'Não é possível deletar este curso pois ele possui vídeos associados.'
        );
      } else {
        AlertService.error('Erro', 'Falha ao deletar o curso.');
      }
      return false;
    } finally {
      setLoading(false);
    }
  };

  return {
    courses: coursesData?.items ?? [],
    totalCount: coursesData?.totalCount ?? 0,
    currentPage: coursesData?.pageNumber ?? 1,
    totalPages: coursesData?.totalPages ?? 0,
    loading,
    fetchCourses,
    createCourse,
    updateCourse,
    deleteCourse
  };
};
