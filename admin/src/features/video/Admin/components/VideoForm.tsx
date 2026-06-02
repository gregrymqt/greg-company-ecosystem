import React from 'react';
import type { VideoFormData, VideoDto } from '@/features/video';
// Reutiliza estilo do container de form se quiser, ou cria um novo. 
// Usaremos um module específico aqui para manter o padrão.
import styles from '../styles/VideoForm.module.scss'; 
import { type FormField, GenericForm } from '@/components/Form/GenericForm';

export interface VideoFormProps {
  initialData?: VideoDto;
  courses: { name: string; publicId: string }[];
  isLoading: boolean;
  onSubmit: (data: VideoFormData) => void;
  onCancel: () => void;
}

export const VideoForm: React.FC<VideoFormProps> = ({
  initialData,
  courses,
  isLoading,
  onSubmit,
  onCancel
}) => {

  const fields: FormField<VideoFormData>[] = [
    {
      name: 'title',
      label: 'Título do Vídeo',
      type: 'text',
      placeholder: 'Ex: Aula 01 - Instalação',
      validation: { required: 'Título é obrigatório' },
      colSpan: 12
    },
    {
      name: 'courseId',
      label: 'Curso Relacionado',
      type: 'select',
      options: courses.map(c => ({ label: c.name, value: c.publicId })),
      validation: { required: 'Selecione um curso' },
      colSpan: 6
    },
    {
      name: 'videoFile',
      label: 'Arquivo de Vídeo',
      type: 'file',
      accept: 'video/*',
      validation: { required: 'O arquivo de vídeo é obrigatório' },
      colSpan: 12
    },
    {
      name: 'description',
      label: 'Descrição',
      type: 'textarea',
      colSpan: 12
    },
    // Exemplo de campo File caso queira thumbnail
    {
      name: 'thumbnailFile',
      label: 'Capa do Vídeo (Opcional)',
      type: 'file',
      accept: 'image/*',
      colSpan: 12
    }
  ];

  const defaultValues = initialData ? {
    title: initialData.title,
    description: initialData.description || '',
    courseId: initialData.courseName ? String(initialData.courseName) : '', // Convert or map appropriately
  } : {};

  return (
    <div className={styles.formWrapper}> {/* Classe definida no CSS similar ao CourseForm */}
      <h2>
        {initialData ? 'Editar Vídeo' : 'Novo Vídeo'}
      </h2>
      
      <GenericForm<VideoFormData>
        fields={fields}
        onSubmit={onSubmit}
        defaultValues={defaultValues}
        submitText={initialData ? 'Salvar Alterações' : 'Cadastrar Vídeo'}
        isLoading={isLoading}
      >
        <div className={styles.actions}>
            <button 
                type="button" 
                className={styles.cancelBtn}
                onClick={onCancel}
                disabled={isLoading}
            >
                Cancelar
            </button>
        </div>
      </GenericForm>
    </div>
  );
};
