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
  courseName?: string;
  thumbnailUrl?: string;
}

/**
 * Payload para criar vídeo (Admin)
 * Baseado em CreateVideoDto.cs
 */
export interface CreateVideoDto {
  title: string;
  description: string;
  courseId: number;
  videoFile: File; // IFormFile obrigatório
  thumbnailFile?: File; // IFormFile opcional
}

/**
 * Payload para atualizar vídeo (Admin)
 * Baseado em UpdateVideoDto.cs
 */
export interface UpdateVideoDto {
  title: string;
  description?: string;
  thumbnailFile?: File; // IFormFile opcional
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
 * Filtros para listagem de vídeos (Admin)
 */
export interface VideoFilters {
  page: number;
  pageSize: number;
  courseId?: number;
  status?: VideoStatus;
}

// =================================================================
// UI HELPERS - Para componentes de formulário
// =================================================================

/**
 * Dados do formulário de vídeo (compatível com React Hook Form)
 */
export interface VideoFormData {
  title: string;
  description: string;
  courseId: string; // Como string para compatibilidade com select
  videoFile?: FileList;
  thumbnailFile?: FileList;
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
}

/**
 * Helper para construir URL do manifest HLS
 */
export const getManifestUrl = (storageIdentifier: string): string => {
  return `/api/videos/${storageIdentifier}/manifest.m3u8`;
};
