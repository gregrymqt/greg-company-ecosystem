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
} from '@/features/course/types/course.types';
import { STORAGE_KEYS, StorageService } from '@/shared/services/storage.service';

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
  getAllPaginated: async (pageNumber: number, pageSize: number = 5, name?: string) => {
    const endpoint = `${BASE_ENDPOINT}/paginated?pageNumber=${pageNumber}&pageSize=${pageSize}${name ? `&name=${name}` : ''}`;
    const response = await ApiService.get<PaginatedResponse<CourseDto>>(endpoint);

    // Adapter Pattern: Transforma DTO (Backend) em UI Model (Frontend)
    const uiData: CourseRowUI[] = response.items.map((course) => {
      // StorageService já possui try/catch interno e faz o parse do JSON automaticamente.
      // Retorna null caso não exista a chave ou ocorra erro.
      const lastWatchedVideo = StorageService.getItem<VideoCardUI>(
        STORAGE_KEYS.LAST_WATCHED_COURSE + course.publicId
      );

      return {
        id: course.publicId,
        categoryName: course.name,
        description: course.description || '',
        year: course.year || '',
        creator: course.creator || '',
        lastWatchedVideo,
        videos: course.videos.map((video): VideoCardUI => ({
          id: video.publicId,
          title: video.title,
          description: video.description,
          thumbnailUrl: video.thumbnailUrl || '/assets/placeholder.jpg',
          durationFormatted: formatDuration(video.duration?.totalSeconds || 0),
          courseId: video.courseId,
          isNew: false,
          status: video.status || "Pending"
        }))
      };
    });

    return {
      data: uiData,
      totalCount: response.totalCount,
      currentPage: response.pageNumber,
      totalPages: response.totalPages
    };
  },
};
