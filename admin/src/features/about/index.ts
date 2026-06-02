/**
 * Barrel exports para About Feature (Admin)
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
export { useAboutSection } from './hooks/useAboutSection';
export { useTeamMembers } from './hooks/useTeamMembers';

// Components (Admin management)
export { TeamMemberForm } from './components/TeamMemberForm';
export { TeamMemberList } from './components/TeamMemberList';
export { AboutSectionForm } from './components/AboutSectionForm';
export { AboutSectionList } from './components/AboutSectionList';
