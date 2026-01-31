/**
 * Public Video Service
 * Permite usuários assistirem vídeos
 * Alinhado com VideosController.cs
 */

import { ApiService } from '@/shared/services/api.service';
import type { VideoDto, PlayerVideoDto } from '../../shared';

class PublicVideoService {
  private readonly BASE_PATH = '/videos';

  /**
   * Busca os metadados do vídeo por PublicId
   * GET /videos/{publicId}
   */
  async getById(publicId: string): Promise<VideoDto> {
    return await ApiService.get<VideoDto>(`${this.BASE_PATH}/${publicId}`);
  }

  /**
   * Constrói a URL do manifesto HLS
   * GET /api/videos/{storageIdentifier}/manifest.m3u8
   */
  getManifestUrl(storageIdentifier: string): string {
    return `/api/videos/${storageIdentifier}/manifest.m3u8`;
  }

  /**
   * Transforma VideoDto em PlayerVideoDto para UI
   */
  toPlayerDto(video: VideoDto): PlayerVideoDto {
    return {
      id: video.id,
      title: video.title,
      description: video.description,
      thumbnailUrl: video.thumbnailUrl,
      durationFormatted: this.formatDuration(video.duration),
      courseTitle: video.courseName,
      storageIdentifier: video.storageIdentifier
    };
  }

  /**
   * Formata duração de TimeSpan para formato legível
   * Ex: "00:12:30" -> "12:30"
   */
  private formatDuration(duration: string): string {
    const parts = duration.split(':');
    if (parts.length === 3) {
      const hours = parseInt(parts[0], 10);
      const minutes = parts[1];
      const seconds = parts[2];
      
      return hours > 0 
        ? `${hours}:${minutes}:${seconds}`
        : `${minutes}:${seconds}`;
    }
    return duration;
  }
}

export const publicVideoService = new PublicVideoService();
