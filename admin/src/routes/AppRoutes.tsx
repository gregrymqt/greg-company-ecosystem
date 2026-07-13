import { Routes, Route, Navigate } from "react-router-dom";
import { ProtectedRoute } from "@/routes/ProtectedRoute";
import { AppRoles } from "@/types/models";
import { AccessDenied } from "@/pages/AccessDenied/AccessDenied";
import { LoginPage } from '@/features/auth/pages';
import { AdminProfile } from "@/pages/Admin/AdminProfile";
import { AdminCourseManager } from '@/features/course/pages/AdminCourseManager';
import { PlansAdmin } from '@/features/plan/pages/PlansAdmin';
import { AdminClaimsPage } from '@/features/claim/pages/AdminClaimsPage';
import { ChargebackManager } from '@/features/chargeback/pages/ChargebackManager';
import { SupportAdminPage } from '@/features/support/pages/SupportAdminPage';
import { AdminHomePage } from '@/features/home/pages/AdminHomePage';
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
          <Route path="/home" element={<AdminHomePage />} />
          <Route path="/planos" element={<PlansAdmin />} />
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
