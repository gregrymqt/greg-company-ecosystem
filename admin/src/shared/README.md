# 📁 shared/

**Objetivo:** Código utilitário compartilhado por todo o painel de administração.

**Exemplos do que colocar aqui:**
- **constants/:** Mapeamento de perfis de usuário (`ROLES`), configurações de paginação padrão, menus do sidebar.
- **contexts/:** Autenticação global de administrador.
- **hooks/:** Funções reutilizáveis no Admin, como `useExportToCsv`.

**Regras:**
- **NÃO** coloque lógica de negócio atrelada a uma funcionalidade de gerência aqui. (Ex: `useBanUser` não pertence ao `shared/`, pertence à *feature* de usuários).
