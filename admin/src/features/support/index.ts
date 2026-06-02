/**
 * Barrel exports para Support Feature (Admin)
 * Siga o padrão: import { useAdminSupport, SupportTicketDto } from '@/features/support'
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

// Admin (Support Management)
export { adminSupportService } from './services/support-admin.service';
export { useAdminSupport } from './hooks/useAdminSupport';
export { SupportTicketList } from './components/SupportTicketList';
