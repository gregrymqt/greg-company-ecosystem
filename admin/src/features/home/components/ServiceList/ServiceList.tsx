import React from 'react';

import styles from './HomeLists.module.scss';
import { ActionMenu } from '@/components/ActionMenu/ActionMenu';
import { type TableColumn, Table } from '@/components/Table/Table';
import type { ServiceDto } from '@/features/home/types/home.types';

interface ServiceListProps {
  data: ServiceDto[];
  isLoading: boolean;
  onEdit: (item: ServiceDto) => void;
  onDelete: (id: number) => void;
}

export const ServiceList: React.FC<ServiceListProps> = ({ 
  data, 
  isLoading, 
  onEdit, 
  onDelete 
}) => {

  const columns: TableColumn<ServiceDto>[] = [
    {
      header: 'Ícone',
      width: '70px',
      render: (item) => (
        <div className={styles.serviceIconWrapper} title={item.icon}>
          {/* Renderiza a classe do ícone (ex: 'fas fa-code') */}
          <i className={item.icon}></i>
        </div>
      )
    },
    {
      header: 'Título',
      accessor: 'title',
      width: '25%'
    },
    {
      header: 'Descrição',
      render: (item) => (
        <span className={styles.truncateText} title={item.description}>
          {item.description}
        </span>
      )
    },
    {
      header: 'Botão',
      accessor: 'ctaText',
      width: '15%'
    },
    {
      header: 'Ações',
      width: '80px',
      render: (item) => (
        <div className={styles.actionsCell}>
          <ActionMenu 
            onEdit={() => onEdit(item)} 
            onDelete={() => onDelete(item.id)} 
          />
        </div>
      )
    }
  ];

  return (
    <div className={styles.listContainer}>
      <Table<ServiceDto>
        data={data}
        columns={columns}
        isLoading={isLoading}
        keyExtractor={(item) => item.id}
        emptyMessage="Nenhum serviço cadastrado."
      />
    </div>
  );
};
