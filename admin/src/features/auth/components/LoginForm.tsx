import { Form } from '@/components/Form/Form';
import { useLoginForm } from '@/features/auth/hooks/useLoginForm';
import { Eye, EyeOff } from 'lucide-react';
import styles from '../styles/LoginForm.module.scss';

export const LoginForm = () => {
  const { formMethods, onSubmit, showPassword, toggleShowPassword, handleForgotPassword } = useLoginForm();

  return (
    <div className={styles.loginForm}>
      <h2 className={styles.title}>Entrar</h2>
      
      <Form formMethods={formMethods} onSubmit={onSubmit}>
        <Form.Input
          name="email"
          label="Email"
          type="email"
          placeholder="seu@email.com"
        />

        <div className={styles.passwordWrapper}>
          <Form.Input
            name="password"
            label="Senha"
            type={showPassword ? 'text' : 'password'}
            placeholder="••••••••"
          />
          <button
            type="button"
            className={styles.passwordToggle}
            onClick={toggleShowPassword}
            aria-label="Alternar visibilidade da senha"
          >
            {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
          </button>
        </div>

        <div className={styles.forgotPasswordWrapper}>
          <a href="#" className={styles.forgotPassword} onClick={handleForgotPassword}>
            Esqueci minha senha
          </a>
        </div>

        {formMethods.formState.errors.root && (
          <div className={styles.error}>
            {formMethods.formState.errors.root.message}
          </div>
        )}

        <Form.Actions>
          <Form.Submit isLoading={formMethods.formState.isSubmitting} className={styles.submitButton}>
            Entrar
          </Form.Submit>
        </Form.Actions>
      </Form>
    </div>
  );
};
