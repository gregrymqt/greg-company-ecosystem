import React from 'react';
import styles from './HomeForms.module.scss';
import { Form } from '@/components/Form';
import type { ServiceDto, ServiceFormValues } from '@/features/home/types/home.types';

interface ServiceFormProps {
  initialData?: ServiceDto;
  onSubmit: (data: ServiceFormValues) => void;
  isLoading?: boolean;
  onCancel: () => void;
}

export const ServiceForm: React.FC<ServiceFormProps> = ({ 
  initialData, 
  onSubmit, 
  isLoading,
  onCancel
}) => {
  const isEditing = !!initialData;

  const defaultValues: Partial<ServiceFormValues> = initialData ? {
    title: initialData.title,
    description: initialData.description,
    icon: initialData.icon,
    ctaText: initialData.ctaText,
    ctaLink: initialData.ctaLink,
    order: initialData.order
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
          name="icon" 
          label="Classe do Ícone (FontAwesome)" 
          placeholder="Ex: fas fa-check" 
          validation={{ required: 'A classe do ícone é obrigatória' }} 
          colSpan={12} 
        />

        <Form.Input 
          name="ctaText" 
          label="Texto do Botão" 
          placeholder="Ex: Saber mais" 
          validation={{ required: 'O texto do botão é obrigatório' }} 
          colSpan={6} 
        />

        <Form.Input 
          name="ctaLink" 
          label="Link do Botão" 
          placeholder="Ex: /servicos" 
          validation={{ required: 'O link do botão é obrigatório' }} 
          colSpan={6} 
        />

        <Form.Actions>
          <button 
            type="button" 
            className={styles.cancelBtn} 
            onClick={onCancel}
            disabled={isLoading}
          >
            Cancelar
          </button>
          <Form.Submit isLoading={isLoading}>
            {isEditing ? "Salvar Alterações" : "Criar Serviço"}
          </Form.Submit>
        </Form.Actions>
      </Form>
    </div>
  );
};
