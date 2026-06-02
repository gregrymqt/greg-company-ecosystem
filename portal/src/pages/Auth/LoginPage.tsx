import { useForm } from 'react-hook-form';
import { Form } from '@/components/Form/Form';
import { authService } from '@/features/auth/services/auth.service';
import { useAuth } from '@/features/auth/hooks/useAuth';
import styles from './LoginPage.module.scss';

interface LoginFormData {
  email: string;
  password: string;
}

export const LoginPage = () => {
  const formMethods = useForm<LoginFormData>({
    defaultValues: {
      email: '',
      password: ''
    }
  });
  const { setSession } = useAuth();

  const onSubmit = async (data: LoginFormData) => {
    try {
      const response = await authService.loginWithEmail(data);
      
      setSession({
        ...response.user,
        token: response.token,
        refreshToken: response.refreshToken,
        expiration: response.expiration
      });

      window.location.href = '/';
    } catch (error) {
      console.error('Erro ao fazer login:', error);
      formMethods.setError('root', {
        type: 'manual',
        message: 'Email ou senha inválidos'
      });
    }
  };

  const handleGoogleLogin = () => {
    authService.loginGoogle();
  };

  return (
    <div className={styles.loginPage}>
      <div className={styles.loginCard}>
        <h2 className={styles.title}>Entrar</h2>
        
        <Form formMethods={formMethods} onSubmit={onSubmit}>
          <Form.Input
            name="email"
            label="Email"
            type="email"
            placeholder="seu@email.com"
            rules={{
              required: 'Email é obrigatório',
              pattern: {
                value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                message: 'Email inválido'
              }
            }}
          />

          <Form.Input
            name="password"
            label="Senha"
            type="password"
            placeholder="••••••••"
            rules={{
              required: 'Senha é obrigatória',
              minLength: {
                value: 6,
                message: 'Senha deve ter no mínimo 6 caracteres'
              }
            }}
          />

          {formMethods.formState.errors.root && (
            <div className={styles.error}>
              {formMethods.formState.errors.root.message}
            </div>
          )}

          <Form.Actions>
            <Form.Submit isLoading={formMethods.formState.isSubmitting}>
              Entrar
            </Form.Submit>
          </Form.Actions>
        </Form>

        <div className={styles.divider}>
          <span>ou</span>
        </div>

        <button
          type="button"
          className={styles.googleButton}
          onClick={handleGoogleLogin}
        >
          <svg className={styles.googleIcon} viewBox="0 0 24 24">
            <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
            <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
            <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
            <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
          </svg>
          Continuar com Google
        </button>
      </div>
    </div>
  );
};
