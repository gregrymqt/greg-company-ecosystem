import React from 'react';
// CORREÇÃO 1: Importação correta (GenericForm como valor)

import styles from '../styles/AboutForm.module.scss';
import { Form } from '@/components/Form';
import type { TeamMember, TeamMemberFormValues } from '@/features/about/types/about.types';

interface MemberFormProps {
  initialData?: TeamMember;
  onSubmit: (data: TeamMemberFormValues) => void;
  isLoading?: boolean;
}

export const TeamMemberForm: React.FC<MemberFormProps> = ({ 
  initialData, 
  onSubmit, 
  isLoading 
}) => {

  const isEditing = !!initialData;

  // CORREÇÃO 2: Substituído 'any' por 'Partial<TeamMemberFormValues>'
  const defaultValues: Partial<TeamMemberFormValues> = initialData ? {
    name: initialData.name,
    role: initialData.role,
    linkedinUrl: initialData.linkedinUrl,
    githubUrl: initialData.githubUrl
  } : {};

  return (
    <div className={styles.formContainer}>
      <div className={styles.header}>
        <h2>{isEditing ? 'Editar Membro' : 'Adicionar ao Time'}</h2>
        <p>Cadastre ou atualize as informações do colaborador.</p>
      </div>

      {isEditing && initialData?.photoUrl && (
        <div className={`${styles.imagePreviewHint} ${styles.warning}`}>
          <i className="fas fa-info-circle"></i>
          <span>
            Imagem atual configurada. Envie um arquivo apenas para substituí-la.
          </span>
        </div>
      )}

      <Form<TeamMemberFormValues>
        onSubmit={onSubmit}
        defaultValues={defaultValues}
      >
        <Form.Input 
          name="name" 
          label="Nome do Colaborador" 
          placeholder="Ex: Ana Silva" 
          validation={{ required: 'O nome é obrigatório' }} 
          colSpan={6} 
        />
        
        <Form.Input 
          name="role" 
          label="Cargo / Função" 
          placeholder="Ex: Tech Lead" 
          validation={{ required: 'O cargo é obrigatório' }} 
          colSpan={6} 
        />
        
        <Form.Input 
          name="newPhoto" 
          label={isEditing ? 'Nova Foto (Opcional)' : 'Foto de Perfil'} 
          type="file" 
          accept="image/*" 
          validation={{ required: isEditing ? false : 'A foto é obrigatória' }} 
          colSpan={12} 
        />
        
        <Form.Input 
          name="linkedinUrl" 
          label="LinkedIn (URL)" 
          placeholder="https://linkedin.com/in/..." 
          colSpan={6} 
        />
        
        <Form.Input 
          name="githubUrl" 
          label="GitHub (URL)" 
          placeholder="https://github.com/..." 
          colSpan={6} 
        />

        <Form.Actions>
          <Form.Submit isLoading={isLoading}>
            {isEditing ? "Atualizar Membro" : "Adicionar Membro"}
          </Form.Submit>
        </Form.Actions>
      </Form>
    </div>
  );
};
