import { useForm } from 'react-hook-form';
import { authService } from '@/features/auth/services/auth.service';
import { useAuth } from '@/features/auth/hooks/useAuth';
import type { RegisterFormData } from '@/features/auth/types/auth.dtos';

export const useRegisterForm = () => {
  const formMethods = useForm<RegisterFormData>({
    defaultValues: {
      name: '',
      email: '',
      password: '',
      confirmPassword: ''
    }
  });
  const { setSession, loginGoogle } = useAuth();

  const onSubmit = async (data: RegisterFormData) => {
    try {
      const response = await authService.register(data);
      
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
      console.error('Erro ao fazer registro:', error);
      formMethods.setError('root', {
        type: 'manual',
        message: 'Erro ao criar conta. Verifique os dados e tente novamente.'
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
