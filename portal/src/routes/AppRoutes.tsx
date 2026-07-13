import { Routes, Route, Navigate } from "react-router-dom";
import { ProtectedRoute } from "@/routes/ProtectedRoute";
import { SubscriptionRoute } from "@/routes/SubscriptionRoute";
import { AccessDenied } from "@/pages/AccessDenied/AccessDenied";
import { GoogleCallbackPage, LoginPage } from "@/pages/Auth";
import { Home } from "@/features/home/pages/Home";
import { CourseFeed } from "@/features/course/pages/CourseFeed";
import { PlayerScreen } from "@/features/video/pages/PlayerScreen";
import { UserClaimsPage } from "@/features/Claim/pages/UserClaimsPage";
import { WalletPage } from "@/features/Wallet/pages/WalletPage";
import { TransactionsPage } from "@/features/Transactions/pages/TransactionsPage";
import { SubscriptionPage } from "@/features/Subscription/pages/SubscriptionPage";
import { ProfileDashboard } from "@/features/profile/pages/ProfileDashboard";
import { CreateSupportPage } from "@/features/support/pages/CreateSupportPage";
import { AboutPage } from "@/features/about/pages/AboutPage";
import { MainLayout } from "@/components/layout/components/MainLayout";
import { PlanPage } from "@/features/Plan/pages/PlanPage";
import { FreeSamplePage } from "@/features/free-sample/pages/FreeSamplePage";
import { PaymentLayout } from "@/features/Payment/pages/PaymentLayout";

export const AppRoutes = () => {
  return (
    <Routes>
      {/* === ROTAS PÚBLICAS === */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/acesso-negado" element={<AccessDenied />} />
      <Route path="/suporte/novo" element={<CreateSupportPage />} />
      <Route path="/sobre" element={<AboutPage />} />

      {/* === ROTAS PROTEGIDAS FULL SCREEN (Sem Layout) === */}
      <Route element={<ProtectedRoute />}>
        <Route path="/amostra-gratis" element={<FreeSamplePage />} />
        <Route path="/payment/checkout/:planId" element={<PaymentLayout />} />
      </Route>

      {/* === LAYOUT PRINCIPAL === */}
      <Route element={<MainLayout />}>
        <Route path="/" element={<Home />} />

        {/* Nível 1: Apenas Autenticado (Logado) */}
        <Route element={<ProtectedRoute />}>
          {/* Rotas que todo logado pode ver */}
          <Route path="/perfil" element={<ProfileDashboard />} />
          <Route path="/plans" element={<PlanPage />} />
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
