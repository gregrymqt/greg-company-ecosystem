/**
 * Barrel exports para Admin (Video Management)
 */

// Services
export { adminVideoService } from './services/video-admin.service.ts';

// Hooks
export { useAdminVideos } from './hooks/useAdminVideos.ts';

// Components - Temporariamente exportando apenas o que foi migrado
// Os componentes VideoList, VideoForm, ProcessingBadge devem ser migrados manualmente
// preservando suas funcionalidades específicas
