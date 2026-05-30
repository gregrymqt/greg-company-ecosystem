/**
 * Public Course Service - Endpoints para área pública
 * Alinhado com CoursesPublicController.cs do Backend
 */

import { ApiService } from '@/shared/services/api.service';
import type { 
  CourseDto, 
  PaginatedResponse, 
  CourseRowUI, 
  VideoCardUI 
} from '@/features/course/shared/types/course.types';

const BASE_ENDPOINT = '/public/courses';

// Helper para formatar duração de vídeos
const formatDuration = (totalSeconds: number): string => {
  if (!totalSeconds) return '';
  const minutes = Math.floor(totalSeconds / 60);
  const hours = Math.floor(minutes / 60);
  if (hours > 0) return `${hours}h ${minutes % 60}m`;
  return `${minutes}m`;
};

export const publicCourseService = {
  /**
   * Busca cursos paginados e transforma para UI
   * GET: /api/public/courses/paginated
   */
  getAllPaginated: async (pageNumber: number, pageSize: number = 5) => {
    const endpoint = `${BASE_ENDPOINT}/paginated?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    const response = await ApiService.get<PaginatedResponse<CourseDto>>(endpoint);

    // Adapter Pattern: Transforma DTO (Backend) em UI Model (Frontend)
    const uiData: CourseRowUI[] = response.items.map((course) => ({
      id: course.publicId,
      categoryName: course.name,
      videos: (course.videos || []).map((video): VideoCardUI => ({
        id: video.publicId,
        title: video.title,
        thumbnailUrl: video.thumbnailUrl || '/assets/placeholder.jpg',
        durationFormatted: formatDuration(video.duration?.totalSeconds || 0),
        courseId: video.courseId,
        isNew: false
      }))
    }));

    return {
      data: uiData,
      totalCount: response.totalCount,
      currentPage: response.pageNumber,
      totalPages: response.totalPages
    };
  },

  /**
   * Busca um curso específico por ID
   * GET: /api/public/courses/{id}
   */
  getById: async (id: string): Promise<CourseDto> => {
    return await ApiService.get<CourseDto>(`${BASE_ENDPOINT}/${id}`);
  },

  /**
   * Busca todos os cursos (sem paginação)
   * GET: /api/public/courses
   */
  getAll: async (): Promise<CourseDto[]> => {
    return await ApiService.get<CourseDto[]>(BASE_ENDPOINT);
  }
};
