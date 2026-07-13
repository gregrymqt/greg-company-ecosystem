/**
 * Barrel exports para Support Feature (Portal - Public)
 * Siga o padrão: import { useUserSupport, SupportTicketDto } from '@/features/support'
 */

// Shared Types
export type * from './types/support.types';

// Public (User Support)
export { userSupportService } from './services/support-user.service';
export { useUserSupport } from './hooks/useUserSupport';
export { SupportCreateForm } from './components/SupportCreateForm/SupportCreateForm';

// Pages
export { CreateSupportPage } from './pages/CreateSupportPage';
