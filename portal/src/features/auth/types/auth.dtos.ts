// features/auth/types/auth.dtos.ts
import type { UserSession } from './auth.types';

// O que recebemos de volta da API de Login
export interface LoginResponse {
  user: UserSession;
  token: string;
  refreshToken: string;
  expiration: string;
}

export interface LoginFormData {
  email: string;
  password: string;
}

export interface RegisterFormData {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
}
