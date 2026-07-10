import React, { useMemo } from 'react';
import { Form } from '@/components/Form';
import type { PixDocType } from '../../types';

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
  


  const defaultValues = useMemo<Partial<PixFormData>>(() => {
    return {
      email: defaultEmail || '',
      firstName: defaultName?.split(' ')[0] || '',
      lastName: defaultName?.split(' ').slice(1).join(' ') || ''
    };
  }, [defaultEmail, defaultName]);

  return (
    <Form<PixFormData>
      onSubmit={onSubmit}
      defaultValues={defaultValues}
    >
      <Form.Input name="firstName" label="Nome" validation={{ required: 'Nome é obrigatório' }} colSpan={6} />
      <Form.Input name="lastName" label="Sobrenome" validation={{ required: 'Sobrenome é obrigatório' }} colSpan={6} />
      <Form.Input name="email" label="E-mail" type="email" validation={{ required: 'E-mail é obrigatório', pattern: { value: /^\S+@\S+$/i, message: 'E-mail inválido' } }} colSpan={12} />
      <Form.Select name="identificationType" label="Tipo de Documento" options={docTypes.map(d => ({ label: d.name, value: d.id }))} validation={{ required: 'Selecione o tipo' }} colSpan={6} />
      <Form.Input name="identificationNumber" label="Número do Documento" validation={{ required: 'Documento é obrigatório' }} colSpan={6} />

      <Form.Actions>
        <Form.Submit isLoading={isLoading}>Gerar QR Code PIX</Form.Submit>
      </Form.Actions>
    </Form>
  );
};
