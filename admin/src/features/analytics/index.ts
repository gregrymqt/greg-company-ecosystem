// @ts-nocheck
/**
 * Barrel exports para Analytics Feature
 */

// Types
export * from './types/analytics.types';
export * from './types/claims.types';
export * from './types/content.types';
export * from './types/financial.types';
export * from './types/subscriptions.types';
export * from './types/support.types';
export * from './types/users.types';
export * from './types/storage.types';

// Services
export * from './services/analytics.service';
export * from './services/claims.service';
export * from './services/content.service';
export * from './services/financial.service';
export * from './services/storage.service';
export * from './services/subscriptions.service';
export * from './services/support.service';
export * from './services/users.service';

// Hooks
export * from './hooks/useAnalytics';
export * from './hooks/useAnalyticsCarousel';
export * from './hooks/useClaimsAnalytics';
export * from './hooks/useContentAnalytics';
export * from './hooks/useFinancialAnalytics';
export * from './hooks/useStorageAnalytics';
export * from './hooks/useSubscriptionsAnalytics';
export * from './hooks/useSupportAnalytics';
export * from './hooks/useUsersAnalytics';

// Components
export * from './components/AnalyticsCarousel';
export * from './components/ProductCard';
