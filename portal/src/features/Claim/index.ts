/**
 * Barrel exports para Claim Feature (Portal - User)
 */

// Types
export type { ClaimSummary } from './types/claims.types';
export type { ReplyFormData } from './types/claim.dtos';

// Services
export { UserClaimService } from './services/userClaim.service';

// Hooks
export { useClaimChatLogic } from './hooks/useClaimChatLogic';

// Components
export { ClaimChat } from './components/ClaimChat/ClaimChat';
export { ClaimsList } from './components/ClaimsList/ClaimsList';
export { ClaimStatusBadge } from './components/ClaimStatusBadge/ClaimStatusBadge';

// Pages
export { UserClaimsPage } from './pages/UserClaimsPage';
