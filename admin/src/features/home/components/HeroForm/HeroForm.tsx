import React from 'react';
import styles from '../styles/HomeForms.module.scss';
import { Form } from '@/components/Form';
import type { HeroSlideDto, HeroFormValues } from '@/features/home/types/home.types';

interface HeroFormProps {
  initialData?: HeroSlideDto;
  onSubmit: (data: HeroFormValues) => void;
  isLoading?: boolean;
  onCancel?: () => void;
}

export const HeroForm: React.FC<HeroFormProps> = ({ 
  initialData, 
  onSubmit, 
  isLoading 
}) => {
  
  const isEditing = !!initialData;

  // CORREÇÃO: Substituído 'any' por 'Partial<HeroFormValues>'
  const defaultValues: Partial<HeroFormValues> = initialData ? {
    title: initialData.title,
    subtitle: initialData.subtitle,
    actionText: initialData.actionText,
    actionUrl: initialData.actionUrl
    // newImage começa undefined na edição, o que é permitido pelo Partial
  } : {};

  return (
    <div className={styles.formContainer}>
      <div className={styles.header}>
        <h3>{isEditing ? 'Editar Slide' : 'Novo Slide Hero'}</h3>
        <p>Configure a imagem e os textos do carrossel principal.</p>
      </div>

      {isEditing && initialData?.imageUrl && (
        <div className={styles.currentImagePreview}>
          <img src={initialData.imageUrl} alt="Atual" />
          <div className="info">
            <strong>Imagem Atual</strong>
            <span>Esta imagem será mantida se você não enviar uma nova.</span>
          </div>
        </div>
      )}

      <Form<HeroFormValues>
        onSubmit={onSubmit}
        defaultValues={defaultValues}
      >
        <Form.Input 
          name="title" 
          label="Título Principal" 
          placeholder="Ex: Bem-vindo à Inovação" 
          validation={{ required: 'O título é obrigatório' }} 
          colSpan={6} 
        />
        
        <Form.Input 
          name="subtitle" 
          label="Subtítulo" 
          placeholder="Ex: Soluções completas para você" 
          validation={{ required: 'O subtítulo é obrigatório' }} 
          colSpan={6} 
        />
        
        <Form.Input 
          name="actionText" 
          label="Texto do Botão (CTA)" 
          placeholder="Ex: Saiba Mais" 
          validation={{ required: 'O texto do botão é obrigatório' }} 
          colSpan={6} 
        />
        
        <Form.Input 
          name="actionUrl" 
          label="Link do Botão" 
          placeholder="Ex: /servicos ou https://..." 
          validation={{ required: 'O link é obrigatório' }} 
          colSpan={6} 
        />
        
        <Form.Input 
          name="newImage" 
          label={isEditing ? 'Alterar Imagem (Deixe vazio para manter)' : 'Imagem do Banner'} 
          type="file" 
          accept="image/png, image/jpeg, image/webp" 
          validation={{ required: isEditing ? false : 'A imagem é obrigatória' }} 
          colSpan={12} 
        />

        <Form.Actions>
          <Form.Submit isLoading={isLoading}>
            {isEditing ? "Salvar Alterações" : "Criar Slide"}
          </Form.Submit>
        </Form.Actions>
      </Form>
    </div>
  );
};
