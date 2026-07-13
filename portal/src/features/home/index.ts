/**
 * Barrel exports para Home Feature (Portal - Public)
 */

// Types
export type { HeroSlideDto, ServiceDto, HomeContentDto } from './types/home.types';

// Services
export { publicHomeService } from './services/publicHome.service';

// Hooks
export { usePublicHomeData } from './hooks/usePublicHomeData';
export { useScrollOpacity } from './hooks/useScrollOpacity';

// Components
export { Hero } from './components/Hero';
export { Services } from './components/Services';

// Pages
export { Home } from './pages/Home';
