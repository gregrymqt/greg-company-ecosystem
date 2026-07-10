import React from 'react';

import  { Card } from '@/components/Card/Card';
import { Form } from '@/components/Form';
import { useAdminTerminal } from '../hooks/useAdminTerminal';
import type { TerminalFormData } from '../types';
import styles from '../styles/AdminTerminal.module.scss';

export const AdminTerminal: React.FC = () => {
  const { handleTerminalCommand } = useAdminTerminal();



  return (
    <Card className={styles.terminalCard}>
      <Card.Body title="Terminal de Comando"> 
         {/* [cite: 39] Usando sub-componente Body */}
        <p className={styles.description}>
            Configure os parâmetros abaixo para inicializar o ambiente administrativo desejado.
        </p>
        
        <Form<TerminalFormData>
          onSubmit={handleTerminalCommand}
        >
          <Form.Select name="module" label="1. Módulo de Acesso" options={[
            { label: 'Gestão de Cursos (CourseAdmin)', value: 'course' },
            { label: 'Gestão de Vídeos (VideoAdmin)', value: 'video' },
            { label: 'Financeiro (MercadoPago)', value: 'mercadopago' },
          ]} validation={{ required: 'Selecione um módulo' }} colSpan={12} />
          
          <Form.Select name="action" label="2. Operação Inicial" options={[
            { label: 'Dashboard / Visão Geral', value: 'dashboard' },
            { label: 'Listagem de Dados', value: 'list' },
            { label: 'Cadastrar Novo', value: 'create' },
          ]} validation={{ required: 'Selecione a ação' }} colSpan={6} />

          <Form.Select name="filter" label="3. Filtro de Dados" options={[
            { label: 'Todos os Registros', value: 'all' },
            { label: 'Apenas Ativos', value: 'active' },
            { label: 'Arquivados / Inativos', value: 'archived' },
          ]} validation={{ required: 'Selecione o filtro' }} colSpan={6} />

          <Form.Actions>
            <Form.Submit>Executar Comando 🚀</Form.Submit>
          </Form.Actions>
        </Form>
      </Card.Body>
    </Card>
  );
};
