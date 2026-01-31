/**
 * Barrel exports para tipos compartilhados de Video
 */

export type {
  VideoStatus,
  VideoDto,
  CreateVideoDto,
  UpdateVideoDto,
  PaginatedVideoResult,
  VideoFilters,
  VideoFormData,
  PlayerVideoDto
} from './types/video.types.ts';

export { getManifestUrl } from './types/video.types.ts';
