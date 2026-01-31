import { ApiService } from '@/shared/services/api.service';
import type { UserSessionDto } from '@/features/auth/types/auth.types';
import type { LoginResponse } from '@/features/auth/types/auth.dtos';

interface LoginCredentials {
  email: string;
  password: string;
}

interface RegisterData {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export const authService = {
  /**
   * Login com Email e Senha
   */
  loginWithEmail: async (credentials: LoginCredentials): Promise<LoginResponse> => {
    return await ApiService.post<LoginResponse>('/auth/login', credentials);
  },

  /**
   * Registro de novo usuário
   */
  register: async (data: RegisterData): Promise<LoginResponse> => {
    return await ApiService.post<LoginResponse>('/auth/register', data);
  },

  /**
   * Redireciona para o Google Login
   */
  loginGoogle: () => {
    const baseUrl = import.meta.env.VITE_GENERAL__BASEURL || 'https://localhost:5045';
    window.location.href = `${baseUrl}/api/auth/google-login`;
  },

  /**
   * Busca os dados LEVES do usuário (Perfil + Flags booleanas)
   * Endpoint: GET /api/auth/me
   * Retorna: UserSessionDto (sem listas pesadas)
   */
  getMe: async (): Promise<UserSessionDto> => {
    // O backend agora retorna apenas dados básicos e os booleanos [cite: 4, 5]
    return await ApiService.get<UserSessionDto>('/auth/me');
  },

  /**
   * Logout no servidor
   */
  logout: async (): Promise<void> => {
    await ApiService.post('/auth/logout', {});
  }
};