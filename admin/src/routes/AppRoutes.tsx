import { Routes, Route, Navigate } from "react-router-dom";
import { ProtectedRoute } from "@/routes/ProtectedRoute";
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
import { AppRoles } from "@/features/auth/types/auth.types";

export const AppRoutes = () => {
  return (
    <Routes>
      {/* === ROTAS PÚBLICAS === */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/acesso-negado" element={<AccessDenied />} />

      {/* === LAYOUT PRINCIPAL (ADMIN) === */}
      <Route element={<MainLayout />}>

        {/* Nível 1: Acesso Comum do Painel (Todos os papéis administrativos) */}
        <Route element={<ProtectedRoute allowedRoles={[AppRoles.Admin, AppRoles.CourseAdmin, AppRoles.EcommerceAdmin, AppRoles.Manager]} />}>
          <Route path="/" element={<AdminProfile />} />
          <Route path="/home" element={<AdminHomePage />} />
          <Route path="/suporte" element={<SupportAdminPage />} />
        </Route>

        {/* Nível 2: Gestão de Cursos e Vídeos */}
        {/* Acesso liberado apenas para CourseAdmin e Admin (programadores) */}
        <Route element={<ProtectedRoute allowedRoles={[AppRoles.Admin, AppRoles.CourseAdmin]} />}>
          <Route path="/cursos" element={<AdminCourseManager />} />
          <Route path="/planos" element={<PlansAdmin />} />
        </Route>

        {/* Nível 3: E-commerce, Reclamações e Financeiro */}
        {/* Acesso liberado apenas para EcommerceAdmin e Admin (programadores) */}
        <Route element={<ProtectedRoute allowedRoles={[AppRoles.Admin, AppRoles.EcommerceAdmin]} />}>
          <Route path="/reclamacoes" element={<AdminClaimsPage />} />
          <Route path="/contestacoes" element={<ChargebackManager />} />
        </Route>

        {/* Nível 4: Relatórios Financeiros Avançados */}
        <Route element={<ProtectedRoute allowedRoles={[AppRoles.Admin, AppRoles.Manager]} />}>
          <Route path="/relatorios" element={<h1>Relatórios Financeiros</h1>} />
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};
