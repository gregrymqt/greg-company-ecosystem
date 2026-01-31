/**
 * Admin Course Service - Endpoints para área administrativa
 * Alinhado com CoursesAdminController.cs do Backend
 */

import { ApiService } from '@/shared/services/api.service';
import type { 
  CourseDto, 
  CourseFilters, 
  PaginatedResponse, 
  CreateCourseData, 
  UpdateCourseData 
} from '@/features/course/shared/types/course.types';

const BASE_ENDPOINT = '/admin/courses';

export const adminCourseService = {
  /**
   * Lista cursos com paginação
   * GET: /api/admin/courses
   */
  getAll: async (filters: CourseFilters): Promise<PaginatedResponse<CourseDto>> => {
    const params = new URLSearchParams({
      pageNumber: filters.pageNumber.toString(),
      pageSize: filters.pageSize.toString(),
    });

    if (filters.name) {
      params.append('name', filters.name);
    }

    return await ApiService.get<PaginatedResponse<CourseDto>>(`${BASE_ENDPOINT}?${params.toString()}`);
  },

  /**
   * Busca curso por ID
   * GET: /api/admin/courses/{id}
   */
  getById: async (id: string): Promise<CourseDto> => {
    return await ApiService.get<CourseDto>(`${BASE_ENDPOINT}/${id}`);
  },

  /**
   * Pesquisa cursos por nome
   * GET: /api/admin/courses/search
   */
  search: async (name: string): Promise<CourseDto[]> => {
    return await ApiService.get<CourseDto[]>(`${BASE_ENDPOINT}/search?name=${encodeURIComponent(name)}`);
  },

  /**
   * Cria novo curso
   * POST: /api/admin/courses
   */
  create: async (data: CreateCourseData): Promise<CourseDto> => {
    return await ApiService.post<CourseDto>(BASE_ENDPOINT, data);
  },

  /**
   * Atualiza curso existente
   * PUT: /api/admin/courses/{id}
   */
  update: async (id: string, data: UpdateCourseData): Promise<CourseDto> => {
    return await ApiService.put<CourseDto>(`${BASE_ENDPOINT}/${id}`, data);
  },

  /**
   * Remove curso
   * DELETE: /api/admin/courses/{id}
   */
  delete: async (id: string): Promise<void> => {
    await ApiService.delete(`${BASE_ENDPOINT}/${id}`);
  }
};