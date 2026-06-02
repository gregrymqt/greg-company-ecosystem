/**
 * Barrel exports para Video Feature (Portal)
 * Siga o padrão: import { useVideoPlayer, VideoDto } from '@/features/video'
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

// Public (Video Player)
export {
  publicVideoService,
  useVideoPlayer,
  VideoPlayerFrame,
  VideoMetadata
} from './Public';
