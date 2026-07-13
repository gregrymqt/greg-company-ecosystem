// LoginForm.tsx - CORRIGIDO
import { type FC } from 'react';
import { useForm } from 'react-hook-form';
import * as yup from 'yup';
import styles from '../styles/LoginForm.module.scss';
import { yupResolver } from '@hookform/resolvers/yup';
import { Form } from '@/components/Form';

const loginSchema = yup.object({
  email: yup.string().required('E-mail é obrigatório').email('E-mail inválido'),
  password: yup.string().required('Senha é obrigatória').min(6, 'A senha deve ter pelo menos 6 caracteres'),
});

type LoginFormValues = yup.InferType<typeof loginSchema>;

export const LoginForm: FC = () => {
  const methods = useForm<LoginFormValues>({
    resolver: yupResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  });

  const onSubmit = (data: LoginFormValues) => {
    console.log('Dados do login:', data);
  };

  return (
    <Form onSubmit={onSubmit} formMethods={methods} className={styles.loginForm}>
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