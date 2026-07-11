# 📁 routes/

**Objetivo:** Configurações de navegação do painel Admin.

**O que colocar aqui:**
- Mapeamento principal das rotas (ex: `AdminRouter.tsx`).
- Wrappers de proteção (ex: `RequireAdminRole.tsx`) para garantir que apenas gestores acessem a plataforma.

**Regras:**
- Assegure-se de validar permissões de forma estrita, roteando usuários sem privilégios de volta para o Login.
