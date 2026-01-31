/**
 * Formulário para usuários criarem tickets de suporte
 */

import React, { useMemo } from 'react';
import { useUserSupport } from '../hooks/useUserSupport';
import type { CreateSupportTicketDto } from '../../shared';
import { type FormField, GenericForm } from '@/components/Form/GenericForm';
import styles from '../styles/SupportCreateForm.module.scss';

export const SupportCreateForm: React.FC = () => {
  const { createTicket, loading } = useUserSupport();

  const fields = useMemo((): FormField<CreateSupportTicketDto>[] => [
    {
      name: 'context',
      label: 'Assunto',
      type: 'text',
      placeholder: 'Ex: Problema no acesso ao curso',
      colSpan: 12,
      validation: {
        required: 'O assunto é obrigatório',
        minLength: {
          value: 3,
          message: 'O assunto deve ter no mínimo 3 caracteres'
        }
      }
    },
    {
      name: 'explanation',
      label: 'Descrição do Problema',
      type: 'textarea',
      placeholder: 'Descreva detalhadamente o problema que você está enfrentando...',
      colSpan: 12,
      validation: {
        required: 'A descrição é obrigatória',
        minLength: {
          value: 10,
          message: 'A descrição deve ter no mínimo 10 caracteres'
        }
      }
    }
  ], []);

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2>Criar Ticket de Suporte</h2>
        <p className={styles.subtitle}>
          Descreva seu problema e nossa equipe entrará em contato em breve.
        </p>
      </div>

      <GenericForm<CreateSupportTicketDto>
        fields={fields}
        onSubmit={createTicket}
        submitText="Enviar Ticket"
        isLoading={loading}
      />

      <div className={styles.helpText}>
        <i className="fas fa-info-circle"></i>
        <span>
          Você receberá um retorno em até 24 horas úteis. 
          Verifique também sua caixa de spam.
        </span>
      </div>
    </div>
  );
};
