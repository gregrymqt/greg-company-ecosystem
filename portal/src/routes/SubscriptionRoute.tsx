import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { AppRoles } from "@/types/models";
import { useState, useEffect } from "react";

export const SubscriptionRoute = () => {
  const { user, isAuthenticated } = useAuth();
  const [isChecking, setIsChecking] = useState(true);

  useEffect(() => {
    // Aguarda para garantir que o state de auth hidratou
    const timer = setTimeout(() => setIsChecking(false), 600);
    return () => clearTimeout(timer);
  }, [user]);

  if (isChecking) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[50vh] w-full text-center p-8">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary mb-4"></div>
        <h3 className="text-xl font-semibold text-gray-100">Validando credenciais...</h3>
        <p className="text-sm text-gray-400 mt-2">Verificando o status da sua assinatura.</p>
      </div>
    );
  }

  // Segurança extra: se não estiver logado, manda pro login
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // 1. Admin sempre passa (Regra VIP)
  const isAdmin = user?.roles?.includes(AppRoles.Admin);

  // 2. Verificação Otimizada
  const hasAccess = user?.hasActiveSubscription;

  // Lógica: Admin OU Assinante Ativo
  if (isAdmin || hasAccess) {
    return <Outlet />;
  }

  // Se falhar, manda para a vitrine de planos passando a persona (Upsell)
  return <Navigate to="/plans?type=course" replace />;
};
