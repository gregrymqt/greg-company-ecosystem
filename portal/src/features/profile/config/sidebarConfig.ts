import type { SidebarItem } from '@/components/SideBar';
import type { UserSession } from '@/features/auth/types/auth.types';

export const getProfileSidebarItems = (user: UserSession | null): SidebarItem[] => {
  const items: SidebarItem[] = [
    {
      id: 'info',
      label: 'Dados do Perfil',
      icon: 'fas fa-user',
    },
  ];

  if (user?.hasPaymentHistory) {
    items.push({
      id: 'payments',
      label: 'Pagamentos',
      icon: 'fas fa-credit-card',
    });
  }

  if (user?.hasActiveSubscription) {
    items.push({
      id: 'subscription',
      label: 'Assinatura',
      icon: 'fas fa-crown',
    });
  }

  return items;
};