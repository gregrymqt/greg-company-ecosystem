/**
 * Barrel exports para About Feature (Portal - Public)
 */

// Types
export type {
  AboutContentType,
  AboutSectionData,
  TeamMember,
  AboutTeamData,
  AboutPageResponse,
  AboutSectionContent,
} from './types/about.types';

// Services
export { AboutService } from './services/about.service';

// Hooks
export { useAboutData } from './hooks/useAboutData';

// Components (Public display)
export { AboutTeamSection } from './components/TeamMemberSection/TeamMemberSection';
export { AboutHeroSection } from './components/AboutHeroSection/AboutHeroSection';

// Pages
export { AboutPage } from './pages/AboutPage';
