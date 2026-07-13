import React, { useState } from 'react';
import { Sidebar } from '@/components/SideBar';
import type { SidebarItem } from '@/components/SideBar/types/sidebar.types';
import { useAdminSubscription } from '../hooks/useAdminSubscription';
import { AdminSubscriptionList } from '../components/AdminSubscriptionList/AdminSubscriptionList';
import type { AdminSubscriptionDetail } from '../types/subscriptions.types';

import styles from './styles/AdminSubscriptionPage.module.scss';

const SIDEBAR_ITEMS: SidebarItem[] = [
  { id: 'list', label: 'Assinaturas', icon: 'fas fa-file-invoice-dollar' }
];

export const AdminSubscriptionPage: React.FC = () => {
  const {
    subscription,
    loading,
    searchSubscription,
    updateValue,
    updateStatus
  } = useAdminSubscription();

  const [activeSidebarId, setActiveSidebarId] = useState<string | number>('list');
  const [isManaging, setIsManaging] = useState(false);
  const [newAmount, setNewAmount] = useState<number | ''>('');
  const [newStatus, setNewStatus] = useState<'authorized' | 'paused' | 'cancelled' | ''>('');

  const handleSearch = (query: string) => {
    setIsManaging(false);
    searchSubscription(query);
  };

  const handleManageClick = (item: AdminSubscriptionDetail) => {
    setNewAmount(item.auto_recurring?.transaction_amount || '');
    setNewStatus(item.status as 'authorized' | 'paused' | 'cancelled' || '');
    setIsManaging(true);
  };

  const handleUpdateValue = async () => {
    if (typeof newAmount === 'number' && newAmount > 0) {
      const success = await updateValue(newAmount);
      if (success) {
        setIsManaging(false);
      }
    }
  };

  const handleUpdateStatus = async () => {
    if (newStatus && ['authorized', 'paused', 'cancelled'].includes(newStatus)) {
      const success = await updateStatus(newStatus as 'authorized' | 'paused' | 'cancelled');
      if (success) {
        setIsManaging(false);
      }
    }
  };

  return (
    <div className={styles.pageContainer}>
      <Sidebar
        activeItemId={activeSidebarId}
        onItemClick={(item) => setActiveSidebarId(item.id)}
        items={SIDEBAR_ITEMS}
      >
        <div className={styles.contentArea}>
          {activeSidebarId === 'list' && (
            <AdminSubscriptionList
              subscription={subscription}
              loading={loading}
              onSearch={handleSearch}
              onManageClick={handleManageClick}
            />
          )}
        </div>

        {subscription && isManaging && (
          <div className={styles.managementCard}>
            <h3>Gerenciar Assinatura</h3>
            
            <div className={styles.formGroup}>
              <label>Valor da Assinatura</label>
              <input 
                type="number" 
                value={newAmount}
                onChange={(e) => setNewAmount(parseFloat(e.target.value) || '')}
                placeholder="Ex: 99.90"
              />
              <button 
                onClick={handleUpdateValue} 
                disabled={loading || !newAmount}
              >
                Atualizar Valor
              </button>
            </div>

            <div className={styles.formGroup}>
              <label>Status</label>
              <select 
                value={newStatus} 
                onChange={(e) => setNewStatus(e.target.value as any)}
              >
                <option value="">Selecione...</option>
                <option value="authorized">Ativo</option>
                <option value="paused">Pausado</option>
                <option value="cancelled">Cancelado</option>
              </select>
              <button 
                onClick={handleUpdateStatus} 
                disabled={loading || !newStatus}
              >
                Atualizar Status
              </button>
            </div>
          </div>
        )}
      </Sidebar>
    </div>
  );
};
