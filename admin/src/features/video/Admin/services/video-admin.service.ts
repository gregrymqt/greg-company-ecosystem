/**
 * Admin Video Service
 * Gerenciamento completo de vídeos
 * Alinhado com AdminVideosController.cs
 */

import { ApiService } from '@/shared/services/api.service';
import type {
  VideoDto,
  CreateVideoDto,
  UpdateVideoDto,
  PaginatedVideoResult,
  VideoFilters
} from '../../shared';

class AdminVideoService {
  private readonly BASE_PATH = '/admin/videos';

  /**
   * Lista todos os vídeos com paginação
   * GET /admin/videos?page=X&pageSize=Y
   */
  async getAll(filters: VideoFilters): Promise<PaginatedVideoResult> {
    const params = new URLSearchParams({
      page: filters.page.toString(),
      pageSize: filters.pageSize.toString()
    });

    if (filters.courseId) {
      params.append('courseId', filters.courseId.toString());
    }

    if (filters.status) {
      params.append('status', filters.status);
    }

    return await ApiService.get<PaginatedVideoResult>(
      `${this.BASE_PATH}?${params.toString()}`
    );
  }

  /**
   * Cria um novo vídeo com upload
   * POST /admin/videos
   */
  async create(data: CreateVideoDto): Promise<VideoDto> {
    const { videoFile, thumbnailFile, ...textDto } = data;

    // O thumbnailFile é enviado junto com o videoFile no postWithFile
    // mas o backend espera apenas File (videoFile) como arquivo principal
    return await ApiService.postWithFile<VideoDto, typeof textDto>(
      this.BASE_PATH,
      textDto,
      [videoFile, ...(thumbnailFile ? [thumbnailFile] : [])],
      'File', // Nome do campo esperado pelo BaseUploadDto
      undefined,
      false // Não bypass smart logic - pode ser arquivo grande
    );
  }

  /**
   * Atualiza um vídeo existente
   * PUT /admin/videos/{id}
   */
  async update(id: string, data: UpdateVideoDto): Promise<VideoDto> {
    const { thumbnailFile, ...dto } = data;

    return await ApiService.putWithFile<VideoDto, typeof dto>(
      `${this.BASE_PATH}/${id}`,
      dto,
      thumbnailFile || null,
      'thumbnailFile',
      undefined,
      true // Bypass smart logic - thumbnail é pequena
    );
  }

  /**
   * Deleta um vídeo
   * DELETE /admin/videos/{id}
   */
  async delete(id: string): Promise<void> {
    await ApiService.delete(`${this.BASE_PATH}/${id}`);
  }
}

export const adminVideoService = new AdminVideoService();
