/**
 * Tipagem compartilhada de Video - Alinhada com DTOs do Backend
 * 
 * Esta é a fonte única de verdade (Single Source of Truth) para tipos de Video.
 * Tanto Admin quanto Public devem importar daqui.
 */

// =================================================================
// ENUMS E STATUS
// =================================================================

export type VideoStatus = 'Processing' | 'Available' | 'Error';

// =================================================================
// DTOs - Baseados em VideoDto.cs
// =================================================================

/**
 * Video completo (response do backend)
 * Baseado em VideoDto.cs
 */
export interface VideoDto {
  id: string; // GUID (PublicId)
  title: string;
  description?: string;
  storageIdentifier: string;
  uploadDate: string; // ISO 8601 date string
  duration: string; // TimeSpan como string (ex: "00:12:30")
  status: VideoStatus;
  courseId?: number;
  courseName?: string;
  thumbnailUrl?: string;
  nextVideoId?: string;
  prevVideoId?: string;
}


// =================================================================
// PAGINAÇÃO
// =================================================================

/**
 * Resultado paginado de vídeos
 * Baseado em PaginatedResultDto<T>.cs
 */
export interface PaginatedVideoResult {
  items: VideoDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}


/**
 * Interface para Player (Public)
 * Usado para exibição de vídeos
 */
export interface PlayerVideoDto {
  id: string; // PublicId
  title: string;
  description?: string;
  thumbnailUrl?: string;
  durationFormatted: string; // Ex: "12:30"
  courseTitle?: string;
  storageIdentifier: string; // Para construir URL do manifest
  status: VideoStatus; // Para acompanhar o processamento
  nextVideoId?: string;
  prevVideoId?: string;
}

/**
 * Helper para construir URL do manifest HLS
 */
export const getManifestUrl = (storageIdentifier: string): string => {
  return `/api/videos/${storageIdentifier}/manifest.m3u8`;
};
