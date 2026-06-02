import { Routes, Route, Navigate } from "react-router-dom";
import { ProtectedRoute } from "@/routes/ProtectedRoute";
import { AppRoles } from "@/types/models";
import { AccessDenied } from "@/pages/AccessDenied/AccessDenied";
import { LoginPage } from "@/pages/Auth";
import { AdminProfile } from "@/pages/Admin/AdminProfile";
import { AdminCourseManager } from "@/pages/Courses/AdminCourseManager";
import { PlansAdmin } from "@/pages/Plans/PlansAdmin";
import { AdminClaimsPage } from "@/pages/Claims/AdminClaimsPage";
import { ChargebackManager } from "@/pages/ChargeBack/ChargebackManager";
import { SupportAdminPage } from "@/pages/Support/SupportAdminPage";
import { AdminAboutPage } from "@/pages/About/AdminAboutPage";
import { AdminHomePage } from "@/pages/Home/AdminHomePage";
import { AnalyticsPage } from "@/pages/Analytics/AnalyticsPage";
import { MainLayout } from "@/components/layout/components/MainLayout";

export const AppRoutes = () => {
  return (
    <Routes>
      {/* === ROTAS PÚBLICAS === */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/acesso-negado" element={<AccessDenied />} />

      {/* === LAYOUT PRINCIPAL (ADMIN) === */}
      <Route element={<MainLayout />}>
        {/* Nível: Apenas ADMIN */}
        <Route element={<ProtectedRoute allowedRoles={[AppRoles.Admin]} />}>
          <Route path="/" element={<AdminProfile />} />
          <Route path="/cursos" element={<AdminCourseManager />} />
          <Route path="/reclamacoes" element={<AdminClaimsPage />} />
          <Route path="/contestacoes" element={<ChargebackManager />} />
          <Route path="/suporte" element={<SupportAdminPage />} />
          <Route path="/about" element={<AdminAboutPage />} />
          <Route path="/home" element={<AdminHomePage />} />
          <Route path="/planos" element={<PlansAdmin />} />
          <Route path="/analytics" element={<AnalyticsPage />} />
        </Route>

        {/* Nível: Manager OU Admin */}
        <Route
          element={
            <ProtectedRoute allowedRoles={[AppRoles.Admin, AppRoles.Manager]} />
          }
        >
          <Route path="/relatorios" element={<h1>Relatórios Financeiros</h1>} />
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};
