/**
 * Barrel exports para Profile Feature (Portal)
 */

// Types
export * from './types/profile.types';

// Services
export * from './services/user-account.service';

// Hooks
export * from './hooks/useAvatarUpdate';
export * from './hooks/useProfileData';

// Components
export * from './components/AvatarUploadForm/AvatarUploadForm';
export * from './components/UserProfileInfo/UserProfileInfo';

// Pages
export { ProfileDashboard } from './pages/ProfileDashboard';
