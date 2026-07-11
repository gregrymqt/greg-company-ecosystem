# 📁 features/

**Objetivo:** Agrupar código por contexto de domínio do Backoffice (Feature-Sliced Design).

Esta é a pasta mais importante para as regras de negócio de gestão da empresa (ex: `users`, `refunds`, `analytics`, `content_moderation`).

**O que colocar dentro de cada feature (ex: `features/refunds/`):**
- **components/:** Componentes específicos desta feature (ex: `RefundTable.tsx`).
- **services/:** Chamadas de API para o Backend C# ou Bot Python.
- **hooks/:** Lógicas locais da feature.
- **types/:** Interfaces de negócio (ex: `RefundRequest`).

**Regras:**
- Mantenha o escopo isolado. Uma feature não deve ser acoplada a outra. Se houver dependência de dados cruzados, eleve para o `shared/` ou resolva no nível da página (`pages/`).
