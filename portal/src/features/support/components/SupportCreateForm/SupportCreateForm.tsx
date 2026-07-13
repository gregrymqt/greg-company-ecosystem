/**
 * Formulário para usuários criarem tickets de suporte
 */

import React from 'react';
import { useUserSupport } from '../../hooks/useUserSupport';
import type { CreateSupportTicketDto } from '../../types/support.types';
import { Form } from '@/components/Form';
import styles from './SupportCreateForm.module.scss';

export const SupportCreateForm: React.FC = () => {
  const { createTicket, loading } = useUserSupport();



  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2>Criar Ticket de Suporte</h2>
        <p className={styles.subtitle}>
          Descreva seu problema e nossa equipe entrará em contato em breve.
        </p>
      </div>

      <Form<CreateSupportTicketDto>
        onSubmit={createTicket}
      >
        <Form.Input name="context" label="Assunto" placeholder="Ex: Problema no acesso ao curso" validation={{ required: 'O assunto é obrigatório', minLength: { value: 3, message: 'O assunto deve ter no mínimo 3 caracteres' } }} colSpan={12} />
        <Form.Textarea name="explanation" label="Descrição do Problema" placeholder="Descreva detalhadamente o problema que você está enfrentando..." validation={{ required: 'A descrição é obrigatória', minLength: { value: 10, message: 'A descrição deve ter no mínimo 10 caracteres' } }} colSpan={12} />

        <Form.Actions>
          <Form.Submit isLoading={loading}>Enviar Ticket</Form.Submit>
        </Form.Actions>
      </Form>

      <div className={styles.tips}>
        <h4><i className="fas fa-info-circle"></i> Informações Úteis</h4>
        <ul>
          <li>Você receberá um retorno em até 24 horas úteis.</li>
          <li>Verifique também sua caixa de spam.</li>
        </ul>
      </div>
    </div>
  );
};
