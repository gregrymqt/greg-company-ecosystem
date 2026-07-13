import React from 'react';
import styles from './styles/AvatarUploadForm.module.scss';
import { Form } from '@/components/Form';
import { useAvatarUpdate } from '../../hooks/useAvatarUpdate';

interface AvatarFormSchema {
  file: FileList;
}

export const AvatarUploadForm: React.FC = () => {
  const { uploading, updateAvatar } = useAvatarUpdate();

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

      <Form<AvatarFormSchema> onSubmit={handleSubmit}>
        <Form.Input 
          name="file" 
          label="Selecione uma nova foto" 
          type="file" 
          accept="image/png, image/jpeg, image/jpg" 
          validation={{ required: 'A imagem é obrigatória' }} 
          colSpan={12} 
        />

        <Form.Actions>
          <Form.Submit isLoading={uploading}>Atualizar Foto</Form.Submit>
        </Form.Actions>
      </Form>
    </div>
  );
};
