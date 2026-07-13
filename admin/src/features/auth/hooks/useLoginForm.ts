import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { authService } from '@/features/auth/services/auth.service';
import { useAuth } from '@/features/auth/hooks/useAuth';
import type { LoginFormData } from '@/features/auth/types/auth.dtos';
import { useNavigate } from 'react-router-dom';

const loginSchema = yup.object().shape({
  email: yup.string().email('Email inválido').required('Email é obrigatório'),
  password: yup.string().min(6, 'Senha deve ter no mínimo 6 caracteres').required('Senha é obrigatória'),
});

export const useLoginForm = () => {
  const { setSession } = useAuth();
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);

  const toggleShowPassword = () => setShowPassword((prev) => !prev);

  const handleForgotPassword = (e: React.MouseEvent) => {
    e.preventDefault();
    // Navegação ou lógica de "esqueci minha senha" vai aqui
    console.log('Forgot password clicked');
  };
  
  const formMethods = useForm<LoginFormData>({
    resolver: yupResolver(loginSchema),
    defaultValues: {
      email: '',
      password: ''
    }
  });

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
      navigate('/', { replace: true });
    } catch (error) {
      console.error('Erro ao fazer login:', error);
      formMethods.setError('root', {
        type: 'manual',
        message: 'Email ou senha inválidos'
      });
    }
  };

  return {
    formMethods,
    onSubmit,
    showPassword,
    toggleShowPassword,
    handleForgotPassword
  };
};
