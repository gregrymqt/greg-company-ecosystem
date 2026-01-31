import React from 'react';

import styles from '../styles/AvatarUploadForm.module.scss';
import { type FormField, GenericForm } from '@/components/Form/GenericForm';
import { useAvatarUpdate } from '../hooks/useAvatarUpdate';

interface AvatarFormSchema {
  file: FileList;
}

export const AvatarUploadForm: React.FC = () => {
  const { uploading, updateAvatar } = useAvatarUpdate();

  const fields: FormField<AvatarFormSchema>[] = [
    {
      name: 'file',
      label: 'Selecione uma nova foto',
      type: 'file',
      accept: 'image/png, image/jpeg, image/jpg',
      validation: {
        required: 'A imagem é obrigatória'
      }
    }
  ];

  const handleSubmit = async (data: AvatarFormSchema) => {
    if (data.file && data.file.length > 0) {
      await updateAvatar(data.file[0]);
    }
  };

  return (
    <div className={styles.container}>
      <p className={styles.helperText}>
        Formatos aceitos: JPG, PNG. Tamanho máximo: 5MB.
      </p>
      
      <GenericForm<AvatarFormSchema>
        fields={fields}
        onSubmit={handleSubmit}
        submitText="Atualizar Foto"
        isLoading={uploading}
      />
    </div>
  );
};
