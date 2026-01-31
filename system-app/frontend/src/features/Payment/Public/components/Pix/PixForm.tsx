import React, { useMemo } from 'react';
import { type FormField, GenericForm } from '@/components/Form/GenericForm';
import type { PixDocType } from '../../../shared';

interface PixFormData {
  firstName: string;
  lastName: string;
  email: string;
  identificationType: string;
  identificationNumber: string;
}

interface PixFormProps {
  onSubmit: (data: PixFormData) => void;
  isLoading: boolean;
  docTypes: PixDocType[];
  defaultEmail?: string;
  defaultName?: string;
}

export const PixForm: React.FC<PixFormProps> = ({ 
  onSubmit, 
  isLoading, 
  docTypes,
  defaultEmail,
  defaultName
}) => {
  
  const fields = useMemo<FormField<PixFormData>[]>(() => [
    {
      name: 'firstName',
      label: 'Nome',
      type: 'text',
      colSpan: 6,
      validation: { required: 'Nome é obrigatório' }
    },
    {
      name: 'lastName',
      label: 'Sobrenome',
      type: 'text',
      colSpan: 6,
      validation: { required: 'Sobrenome é obrigatório' }
    },
    {
      name: 'email',
      label: 'E-mail',
      type: 'email',
      colSpan: 12,
      validation: { 
        required: 'E-mail é obrigatório',
        pattern: { value: /^\S+@\S+$/i, message: 'E-mail inválido' }
      }
    },
    {
      name: 'identificationType',
      label: 'Tipo de Documento',
      type: 'select',
      colSpan: 6,
      options: docTypes.map(d => ({ label: d.name, value: d.id })),
      validation: { required: 'Selecione o tipo' }
    },
    {
      name: 'identificationNumber',
      label: 'Número do Documento',
      type: 'text',
      colSpan: 6,
      validation: { required: 'Documento é obrigatório' }
    }
  ], [docTypes]);

  const defaultValues = useMemo<Partial<PixFormData>>(() => {
    return {
      email: defaultEmail || '',
      firstName: defaultName?.split(' ')[0] || '',
      lastName: defaultName?.split(' ').slice(1).join(' ') || ''
    };
  }, [defaultEmail, defaultName]);

  return (
    <GenericForm<PixFormData>
      fields={fields}
      onSubmit={onSubmit}
      isLoading={isLoading}
      submitText="Gerar QR Code PIX"
      defaultValues={defaultValues}
    />
  );
};
