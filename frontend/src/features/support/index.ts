/**
 * Barrel exports para Support Feature
 * Siga o padrão: import { useUserSupport, SupportTicketDto } from '@/features/support'
 */

// Shared Types
export type {
  SupportTicketStatus,
  SupportTicketDto,
  CreateSupportTicketDto,
  UpdateSupportTicketDto,
  SupportApiResponse,
  PaginatedSupportResult,
  SupportFilters
} from './types/support.types';

// Public (User Support)
export { userSupportService } from './services/support-user.service';
export { useUserSupport } from './hooks/useUserSupport';
export { SupportCreateForm } from './components/SupportCreateForm';

// Admin (Support Management)
export { adminSupportService } from './services/support-admin.service';
export { useAdminSupport } from './hooks/useAdminSupport';
export { SupportTicketList } from './components/SupportTicketList';
