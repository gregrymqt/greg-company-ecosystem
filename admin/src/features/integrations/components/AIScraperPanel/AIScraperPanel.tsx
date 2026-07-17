import React from 'react';
import { Form } from '@/components/Form/Form';
import { useAiScraper } from '../../hooks/useAiScraper';
import styles from './AIScraperPanel.module.scss';

export const AIScraperPanel: React.FC = () => {
  const { isSavingKey, isStartingScraper, saveCredentials, startScraping, lastTaskId } = useAiScraper();

  return (
    <div className={styles.container}>
      {/* Seção A: Configuração de IA (BYOK) */}
      <section className={styles.section}>
        <h2><i className="fas fa-key"></i> Configuração de IA (BYOK)</h2>
        <Form onSubmit={saveCredentials} defaultValues={{ provider: 'OpenAI (GPT)', access_token: '' }}>
          <Form.Select 
            name="provider" 
            label="Provedor de IA" 
            options={[
              { value: 'OpenAI (GPT)', label: 'OpenAI (GPT)' }, 
              { value: 'Google Gemini', label: 'Google Gemini' }
            ]} 
          />
          <Form.Input name="access_token" label="API Key" type="password" />
          <Form.Actions>
            <Form.Submit isLoading={isSavingKey}>Salvar Credencial</Form.Submit>
          </Form.Actions>
        </Form>
      </section>

      <hr className={styles.divider} />

      {/* Seção B: Disparador de Web Scraping */}
      <section className={styles.section}>
        <h2><i className="fas fa-spider"></i> Disparador de Web Scraping</h2>
        <Form onSubmit={startScraping} defaultValues={{ url: '' }}>
          <Form.Input name="url" label="URL do Produto Concorrente/Fornecedor" />
          <Form.Actions>
            <Form.Submit isLoading={isStartingScraper}>Iniciar Extração com IA</Form.Submit>
          </Form.Actions>
        </Form>
        {lastTaskId && (
          <div style={{ marginTop: '1rem', fontSize: '0.85rem', color: 'var(--text-muted, #666)' }}>
            <i className="fas fa-info-circle" style={{ marginRight: '0.5rem', color: 'var(--primary-color, #007bff)' }}></i>
            Tarefa enfileirada com ID de rastreamento: <code style={{ background: '#f4f4f4', padding: '2px 6px', borderRadius: '4px' }}>{lastTaskId}</code>
          </div>
        )}
      </section>
    </div>
  );
};
