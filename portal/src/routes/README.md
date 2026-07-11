# 📁 routes/

**Objetivo:** Gerenciar as configurações e mapeamentos de navegação da aplicação.

Esta pasta é responsável por conectar as URLs (caminhos no navegador) aos componentes de tela (que ficam na pasta `pages/`).

**Exemplos do que colocar aqui:**
- `index.tsx` ou `AppRouter.tsx`: O mapeamento principal das rotas (usando `react-router-dom` ou biblioteca similar).
- Componentes de Proteção de Rotas: Exemplo `ProtectedRoute.tsx` ou `AuthGuard.tsx` para bloquear acesso de usuários não logados.
- Definições de *layouts* fixos por tipo de rota (ex: `DashboardLayout.tsx` vs `PublicLayout.tsx`).

**Regras:**
- Concentre toda a regra de redirecionamento, guards e lazy loading das páginas aqui.
- Evite misturar UI com a lógica de roteamento puro.
