import React from 'react';
import type { VideoFormData, VideoDto } from '@/features/video';
// Reutiliza estilo do container de form se quiser, ou cria um novo. 
// Usaremos um module específico aqui para manter o padrão.
import styles from '../styles/VideoForm.module.scss'; 
import { Form } from '@/components/Form';

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
      
      <Form<VideoFormData>
        onSubmit={onSubmit}
        defaultValues={defaultValues}
      >
        <Form.Input 
          name="title" 
          label="Título do Vídeo" 
          placeholder="Ex: Aula 01 - Instalação" 
          validation={{ required: 'Título é obrigatório' }} 
          colSpan={12} 
        />
        
        <Form.Select 
          name="courseId" 
          label="Curso Relacionado" 
          options={courses.map(c => ({ label: c.name, value: c.publicId }))} 
          validation={{ required: 'Selecione um curso' }} 
          colSpan={6} 
        />

        <Form.Input 
          name="videoFile" 
          label="Arquivo de Vídeo" 
          type="file" 
          accept="video/*" 
          validation={{ required: 'O arquivo de vídeo é obrigatório' }} 
          colSpan={12} 
        />

        <Form.Textarea 
          name="description" 
          label="Descrição" 
          colSpan={12} 
        />

        <Form.Input 
          name="thumbnailFile" 
          label="Capa do Vídeo (Opcional)" 
          type="file" 
          accept="image/*" 
          colSpan={12} 
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
            {initialData ? 'Salvar Alterações' : 'Cadastrar Vídeo'}
          </Form.Submit>
        </Form.Actions>
      </Form>
    </div>
  );
};
