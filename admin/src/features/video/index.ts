/**
 * Barrel exports para Video Feature (Admin)
 * Siga o padrão: import { useAdminVideos, VideoDto } from '@/features/video'
 */

// Shared Types
export type {
  VideoStatus,
  VideoDto,
  CreateVideoDto,
  UpdateVideoDto,
  PaginatedVideoResult,
  VideoFilters,
  VideoFormData,
  PlayerVideoDto
} from './shared';

export { getManifestUrl } from './shared';

// Admin (Video Management)
export {
  adminVideoService,
  useAdminVideos
} from './Admin';
