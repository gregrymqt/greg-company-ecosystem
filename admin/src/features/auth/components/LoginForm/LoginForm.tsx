// LoginForm.tsx - CORRIGIDO
import { type FC } from 'react';
import styles from './LoginForm.module.scss';
import { Form } from '@/components/Form';
import { useLoginForm } from '../../hooks/useLoginForm';

export const LoginForm: FC = () => {
  const { formMethods, onSubmit } = useLoginForm();

  return (
    <Form onSubmit={onSubmit} formMethods={formMethods} className={styles.loginForm}>
      <Form.Input name="email" label="E-mail" placeholder="seu.email@exemplo.com" />
      <Form.Input name="password" label="Senha" placeholder="••••••••" type="password" />

      <div className={styles.actions}>
        {/* CORREÇÃO: Reativado o uso da classe submitButton */}
        <Form.Submit className={styles.submitButton}>Entrar</Form.Submit>

        <div className={styles.divider}>
          <span>ou</span>
        </div>

        <a href="/login/google" className={styles.googleButton}>
          <img src="/google.svg" alt="Google" />
          Continuar com o Google
        </a>
      </div>
    </Form>
  );
};