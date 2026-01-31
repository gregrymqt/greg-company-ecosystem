/**
 * Barrel exports para Profile Feature
 * Siga o padrão: import { useProfileData, StudentDto } from '@/features/profile'
 */

// Shared Types
export type {
  UserProfileDto,
  AvatarUpdateResponse,
  StudentDto,
  PaginatedResult,
  StudentFilters
} from './shared';

// Account (User Profile)
export {
  userAccountService,
  useProfileData,
  useAvatarUpdate,
  ProfileInfo,
  AvatarUploadForm
} from './Account';

// Admin (Students Management)
export { adminStudentsService, useAdminStudents } from './Admin';
