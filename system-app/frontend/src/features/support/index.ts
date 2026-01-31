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
} from './shared';

// Public (User Support)
export {
  userSupportService,
  useUserSupport,
  SupportCreateForm
} from './Public';

// Admin (Support Management)
export {
  adminSupportService,
  useAdminSupport,
  SupportTicketList
} from './Admin';
