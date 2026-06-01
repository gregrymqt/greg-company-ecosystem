import React from 'react';
import styles from '../styles/HomeForms.module.scss';
import { type FormField, GenericForm } from '@/components/Form/GenericForm';
import type { ServiceDto, ServiceFormValues } from '@/features/home/types/home.types';

interface ServiceFormProps {
  initialData?: ServiceDto;
  onSubmit: (data: ServiceFormValues) => void;
  isLoading?: boolean;
}

export const ServiceForm: React.FC<ServiceFormProps> = ({ 
  initialData, 
  onSubmit, 
  isLoading 
}) => {
  const isEditing = !!initialData;

  const fields: FormField<ServiceFormValues>[] = [
    {
      name: 'title',
      label: 'Título',
      type: 'text',
      placeholder: 'Ex: Suporte Técnico',
      validation: { required: 'O título é obrigatório' },
      colSpan: 12
    },
    {
      name: 'description',
      label: 'Descrição',
      type: 'textarea',
      placeholder: 'Descreva o serviço...',
      validation: { required: 'A descrição é obrigatória' },
      colSpan: 12
    },
    {
      name: 'iconClass',
      label: 'Classe do Ícone (FontAwesome)',
      type: 'text',
      placeholder: 'Ex: fas fa-check',
      validation: { required: 'A classe do ícone é obrigatória' },
      colSpan: 12
    },
    {
      name: 'actionText',
      label: 'Texto do Botão',
      type: 'text',
      placeholder: 'Ex: Saber mais',
      validation: { required: 'O texto do botão é obrigatório' },
      colSpan: 6
    },
    {
      name: 'actionUrl',
      label: 'Link do Botão',
      type: 'text',
      placeholder: 'Ex: /servicos',
      validation: { required: 'O link do botão é obrigatório' },
      colSpan: 6
    }
  ];

  const defaultValues: Partial<ServiceFormValues> = initialData ? {
    title: initialData.title,
    description: initialData.description,
    iconClass: initialData.iconClass,
    actionText: initialData.actionText,
    actionUrl: initialData.actionUrl
  } : {};

  return (
    <div className={styles.formContainer}>
      <div className={styles.header}>
        <h3>{isEditing ? 'Editar Serviço' : 'Novo Serviço'}</h3>
        <p>Configure os detalhes do serviço a ser exibido na home.</p>
      </div>

      <GenericForm<ServiceFormValues>
        fields={fields}
        onSubmit={onSubmit}
        defaultValues={defaultValues}
        isLoading={isLoading}
        submitText={isEditing ? "Salvar Alterações" : "Criar Serviço"}
      />
    </div>
  );
};
