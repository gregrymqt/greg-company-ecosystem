import React from 'react';
import type { CourseFormData, CourseFormProps } from '@/features/course/types/admin-course.types';
import styles from './CourseForm.module.scss';
import { Form } from '@/components/Form';

export const CourseForm: React.FC<CourseFormProps> = ({ 
  initialData, 
  isLoading, 
  onSubmit, 
  onCancel 
}) => {
  
  // Valores padrão se for edição
  const defaultValues = initialData ? {
    name: initialData.name,
    description: initialData.description
  } : {};

  return (
    <div className={styles.formContainer}>
      <div className={styles.formHeader}>
        <h2>
          {initialData ? 'Editar Curso' : 'Novo Curso'}
          {initialData && <span>Editando: {initialData.name}</span>}
        </h2>
      </div>

      <Form<CourseFormData>
        onSubmit={onSubmit}
        defaultValues={defaultValues}
      >
        <Form.Input 
          name="name" 
          label="Nome do Curso" 
          placeholder="Ex: Introdução ao React" 
          validation={{ 
            required: 'O nome do curso é obrigatório',
            minLength: { value: 3, message: 'Mínimo de 3 caracteres' }
          }} 
          colSpan={12} 
        />
        
        <Form.Textarea 
          name="description" 
          label="Descrição Detalhada" 
          placeholder="Descreva o conteúdo do curso..." 
          validation={{ required: 'A descrição é obrigatória' }} 
          colSpan={12} 
        />
        
        <Form.Input 
          name="thumbnail" 
          label="Imagem de Capa (Opcional)" 
          type="file" 
          accept="image/*" 
          colSpan={12} 
        />

        <Form.Actions>
          <div className={styles.actions}>
            <button 
              type="button" 
              className={styles.cancelBtn} 
              onClick={onCancel}
              disabled={isLoading}
            >
              Cancelar
            </button>
            <Form.Submit isLoading={isLoading}>
              {initialData ? 'Atualizar Curso' : 'Criar Curso'}
            </Form.Submit>
          </div>
        </Form.Actions>
      </Form>
    </div>
  );
};
