import { useForm } from 'react-hook-form';
import { authService } from '@/features/auth/services/auth.service';
import { useAuth } from '@/features/auth/hooks/useAuth';
import type { LoginFormData } from '@/features/auth/types/auth.dtos';

export const useLoginForm = () => {
  const formMethods = useForm<LoginFormData>({
    defaultValues: {
      email: '',
      password: ''
    }
  });
  const { setSession, loginGoogle } = useAuth();

  const onSubmit = async (data: LoginFormData) => {
    try {
      const response = await authService.loginWithEmail(data);
      
      // Salva a sessão do usuário
      setSession({
        ...response.user,
        token: response.token,
        refreshToken: response.refreshToken,
        expiration: response.expiration
      });

      // Redireciona para a home ou dashboard
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
    loginGoogle();
  };

  return {
    formMethods,
    onSubmit,
    handleGoogleLogin
  };
};
