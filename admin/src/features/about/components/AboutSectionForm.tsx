import React from "react";
// CORREÇÃO 1: Removido 'type' da importação do GenericForm (ele é um componente/valor)

import styles from '../styles/AboutForm.module.scss';

import { Form } from '@/components/Form';
import type {
  AboutSectionData,
  AboutSectionFormValues,
} from '@/features/about/types/about.types';

interface SectionFormProps {
  initialData?: AboutSectionData;
  onSubmit: (data: AboutSectionFormValues) => void;
  isLoading?: boolean;
}

export const AboutSectionForm: React.FC<SectionFormProps> = ({
  initialData,
  onSubmit,
  isLoading,
}) => {
  const isEditing = !!initialData;

  // CORREÇÃO 2: Substituído 'any' por 'Partial<AboutSectionFormValues>'
  const defaultValues: Partial<AboutSectionFormValues> = initialData
    ? {
        title: initialData.title,
        description: initialData.description,
        imageAlt: initialData.imageAlt,
      }
    : {};

  return (
    <div className={styles.formContainer}>
      <div className={styles.header}>
        <h2>{isEditing ? "Editar Seção" : "Nova Seção Sobre"}</h2>
        <p>Gerencie o conteúdo principal da página About.</p>
      </div>

      {isEditing && initialData?.imageUrl && (
        <div className={`${styles.imagePreviewHint} ${styles.warning}`}>
          <i className="fas fa-info-circle"></i>
          <span>
            Imagem atual configurada. Envie um arquivo apenas para substituí-la.
          </span>
        </div>
      )}

      <Form<AboutSectionFormValues>
        onSubmit={onSubmit}
        defaultValues={defaultValues}
      >
        <Form.Input 
          name="title" 
          label="Título Principal" 
          placeholder="Ex: Nossa História" 
          validation={{ required: "O título é obrigatório" }} 
          colSpan={12} 
        />
        
        <Form.Textarea 
          name="description" 
          label="Conteúdo da Seção" 
          placeholder="Escreva sobre a empresa..." 
          validation={{ required: "A descrição é obrigatória" }} 
          colSpan={12} 
        />
        
        <Form.Input 
          name="newImage" 
          label={isEditing ? "Alterar Imagem (Opcional)" : "Imagem de Destaque"} 
          type="file" 
          accept="image/*" 
          validation={{ required: isEditing ? false : "A imagem é obrigatória" }} 
          colSpan={6} 
        />
        
        <Form.Input 
          name="imageAlt" 
          label="Descrição da Imagem (Alt Text)" 
          placeholder="Descreva a imagem para acessibilidade" 
          validation={{ required: "O texto alternativo é obrigatório para SEO" }} 
          colSpan={6} 
        />

        <Form.Actions>
          <Form.Submit isLoading={isLoading}>
            {isEditing ? "Salvar Alterações" : "Criar Seção"}
          </Form.Submit>
        </Form.Actions>
      </Form>
    </div>
  );
};
