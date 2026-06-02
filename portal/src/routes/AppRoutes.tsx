import { Routes, Route, Navigate } from "react-router-dom";
import { ProtectedRoute } from "@/routes/ProtectedRoute";
import { SubscriptionRoute } from "@/routes/SubscriptionRoute";
import { AccessDenied } from "@/pages/AccessDenied/AccessDenied";
import { GoogleCallbackPage, LoginPage } from "@/pages/Auth";
import { Home } from "@/pages/Home/Home";
import { CourseFeed } from "@/pages/Courses/CourseFeed";
import { PlayerScreen } from "@/pages/Player/PlayerScreen";
import { UserClaimsPage } from "@/pages/Claims/UserClaimsPage";
import { WalletPage } from "@/pages/Wallet/WalletPage";
import { TransactionsPage } from "@/pages/Transactions/TransactionsPage";
import { SubscriptionPage } from "@/pages/Subscription/SubscriptionPage";
import { ProfileDashboard } from "@/pages/Profile/ProfileDashboard";
import { CreateSupportPage } from "@/pages/Support/CreateSupportPage";
import { AboutPage } from "@/pages/About/AboutPage";
import { MainLayout } from "@/components/layout/components/MainLayout";
import { PlanFeed } from "@/pages/Plans/PlanFeed";

export const AppRoutes = () => {
  return (
    <Routes>
      {/* === ROTAS PÚBLICAS === */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/acesso-negado" element={<AccessDenied />} />
      <Route path="/suporte/novo" element={<CreateSupportPage />} />
      <Route path="/sobre" element={<AboutPage />} />

      {/* === LAYOUT PRINCIPAL === */}
      <Route element={<MainLayout />}>
        <Route path="/" element={<Home />} />

        {/* Nível 1: Apenas Autenticado (Logado) */}
        <Route element={<ProtectedRoute />}>
          {/* Rotas que todo logado pode ver */}
          <Route path="/perfil" element={<ProfileDashboard />} />
          <Route path="/plans" element={<PlanFeed />} />
          <Route path="/login/callback" element={<GoogleCallbackPage />} />
          <Route path="reclamacoes" element={<UserClaimsPage />} />

          {/* Nível 1.5: Requer Assinatura Ativa OU Admin */}
          <Route element={<SubscriptionRoute />}>
            <Route path="/cursos" element={<CourseFeed />} />
            <Route path="/player/:videoId" element={<PlayerScreen />} />
            <Route path="/carteira" element={<WalletPage />} />
            <Route path="/transacoes" element={<TransactionsPage />} />
            <Route path="/assinaturas" element={<SubscriptionPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};
