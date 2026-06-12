import React from 'react';
import styles from '../styles/HomeForms.module.scss';
import { Form } from '@/components/Form';
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

      <Form<ServiceFormValues>
        onSubmit={onSubmit}
        defaultValues={defaultValues}
      >
        <Form.Input 
          name="title" 
          label="Título" 
          placeholder="Ex: Suporte Técnico" 
          validation={{ required: 'O título é obrigatório' }} 
          colSpan={12} 
        />

        <Form.Textarea 
          name="description" 
          label="Descrição" 
          placeholder="Descreva o serviço..." 
          validation={{ required: 'A descrição é obrigatória' }} 
          colSpan={12} 
        />

        <Form.Input 
          name="iconClass" 
          label="Classe do Ícone (FontAwesome)" 
          placeholder="Ex: fas fa-check" 
          validation={{ required: 'A classe do ícone é obrigatória' }} 
          colSpan={12} 
        />

        <Form.Input 
          name="actionText" 
          label="Texto do Botão" 
          placeholder="Ex: Saber mais" 
          validation={{ required: 'O texto do botão é obrigatório' }} 
          colSpan={6} 
        />

        <Form.Input 
          name="actionUrl" 
          label="Link do Botão" 
          placeholder="Ex: /servicos" 
          validation={{ required: 'O link do botão é obrigatório' }} 
          colSpan={6} 
        />

        <Form.Actions>
          <Form.Submit isLoading={isLoading}>
            {isEditing ? "Salvar Alterações" : "Criar Serviço"}
          </Form.Submit>
        </Form.Actions>
      </Form>
    </div>
  );
};
