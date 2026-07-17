import React from 'react';
import { Table, type TableColumn } from '@/components/Table/Table';
import { ActionMenu } from '@/components/ActionMenu/ActionMenu';
import type { Product } from '../../types/shared.types'; // Assuming this is where Product comes from
import styles from './SyncProductTable.module.scss';

export interface SyncProductTableProps {
  data: Product[];
  isLoading: boolean;
  provider: 'nuvemshop' | 'shopify';
  onSync: (product: Product) => Promise<void>;
  onUpdate: (id: string | number, data: unknown) => Promise<void>;
  onDelete: (id: string | number) => Promise<void>;
  hasNextPage?: boolean;
  onNextPage?: () => void;
}

export const SyncProductTable: React.FC<SyncProductTableProps> = ({
  data,
  isLoading,
  provider,
  onSync,
  onUpdate,
  onDelete,
  hasNextPage,
  onNextPage
}) => {
  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Exported':
      case 'Processed':
        return <span className={`${styles.badge} ${styles.success}`}>Sincronizado</span>;
      case 'Raw':
      case 'Processing':
        return <span className={`${styles.badge} ${styles.pending}`}>Pendente</span>;
      case 'Failed':
        return <span className={`${styles.badge} ${styles.error}`}>Falhou</span>;
      default:
        return <span className={`${styles.badge} ${styles.default}`}>{status}</span>;
    }
  };

  const columns: TableColumn<Product>[] = [
    {
      header: 'Imagem',
      width: '80px',
      render: (item) => (
        <div className={styles.imageContainer}>
          {item.images && item.images.length > 0 ? (
            <img src={item.images[0]} alt={item.title || item.sku} className={styles.thumbnail} />
          ) : (
            <div className={styles.placeholderImage}>Sem img</div>
          )}
        </div>
      ),
    },
    {
      header: 'Produto',
      render: (item) => (
        <div className={styles.productInfo}>
          <span className={styles.sku}>{item.sku}</span>
          <span className={styles.title}>{item.title || 'Sem título'}</span>
          
          {item.status === 'Failed' && (
            <div className={styles.failedActions}>
              {item.last_error && (
                <div className={styles.errorLog}>
                  <i className="fas fa-exclamation-circle" /> {item.last_error}
                </div>
              )}
              <div className={styles.actionButtons}>
                <button 
                  type="button" 
                  className={styles.rowRetryBtn} 
                  onClick={() => onSync(item)}
                >
                  <i className="fas fa-redo-alt" /> Retentar
                </button>
                <button 
                  type="button" 
                  className={styles.rowManualBtn} 
                  onClick={() => {
                    const id = item._id || item.sku;
                    if (id) onUpdate(id, item);
                  }}
                >
                  <i className="fas fa-pen" /> Mapear Manualmente
                </button>
              </div>
            </div>
          )}
        </div>
      ),
    },
    {
      header: 'Status',
      width: '150px',
      render: (item) => getStatusBadge(item.status),
    },
    {
      header: 'Ações',
      width: '80px',
      align: 'right',
      render: (item) => (
        <ActionMenu
          onEdit={() => {
            const id = item._id || item.sku;
            if (id) {
              onUpdate(id, item);
            }
          }}
          onDelete={() => {
            const id = item._id || item.sku;
            if (id) {
              onDelete(id);
            }
          }}
        >
          {/* Custom Action for Syncing */}
          <button 
            type="button" 
            className={styles.menuItem} 
            onClick={() => onSync(item)}
          >
            <i className="fas fa-sync"></i> Sincronizar
          </button>
          
          {item.status === 'Failed' && (
            <>
              <button 
                type="button" 
                className={styles.menuItem} 
                onClick={() => onSync(item)}
              >
                <i className="fas fa-redo"></i> Retentar Sincronizar
              </button>
              <button 
                type="button" 
                className={styles.menuItem} 
                onClick={() => {
                  const id = item._id || item.sku;
                  if (id) onUpdate(id, item);
                }}
              >
                <i className="fas fa-edit"></i> Mapear Manualmente
              </button>
            </>
          )}
        </ActionMenu>
      ),
    }
  ];

  return (
    <div className={styles.container}>
      <Table<Product>
        data={data}
        columns={columns}
        keyExtractor={(item) => item._id || item.sku || Math.random().toString()}
        isLoading={isLoading}
        emptyMessage={isLoading ? "Carregando..." : "Nenhum produto encontrado na plataforma selecionada."}
      />
      
      {provider === 'shopify' && (
        <div className={styles.pagination}>
          <button className={styles.pageBtn} disabled>
            Anterior
          </button>
          <button 
            className={styles.pageBtn} 
            disabled={!hasNextPage || isLoading} 
            onClick={onNextPage}
          >
            Próximo
          </button>
        </div>
      )}
    </div>
  );
};
