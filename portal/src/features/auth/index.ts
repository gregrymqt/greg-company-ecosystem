/**
 * Barrel exports para Auth Feature (Portal)
 */

// Types
export type { UserSession, UserSessionDto } from './types/auth.types';
export type { LoginResponse, LoginFormData, RegisterFormData } from './types/auth.dtos';

// Services
export { authService } from './services/auth.service';

// Hooks
export { useAuth } from './hooks/useAuth';
export { useLoginForm } from './hooks/useLoginForm';
export { useRegisterForm } from './hooks/useRegisterForm';

// Components
export { LoginForm } from './components/LoginForm/LoginForm';
export { RegisterForm } from './components/RegisterForm/RegisterForm';

// Pages
export { LoginPage } from './pages/LoginPage/LoginPage';
export { RegisterPage } from './pages/RegisterPage/RegisterPage';
export { GoogleCallbackPage } from './pages/GoogleCallbackPage/GoogleCallbackPage';
