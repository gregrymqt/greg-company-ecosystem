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
  AboutSectionFormValues,
  TeamMemberFormValues
} from './types/about.types';

// Services
export { AboutService } from './services/about.service';

// Hooks
export { useAboutData } from './hooks/useAboutData';

// Components (Public display)
export { AboutTeamSection } from './components/TeamMemberSection';
export { AboutHeroSection } from './components/AboutHeroSection';
