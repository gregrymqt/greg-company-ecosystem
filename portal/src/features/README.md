# 📁 features/

**Objetivo:** Agrupar código por contexto de domínio (Feature-Sliced Design).

Esta é a pasta mais importante para as regras de negócio. Em vez de agrupar por "tipo de arquivo" (todas as rotas juntas, todos os serviços juntos), agrupamos por "funcionalidade" (ex: `auth`, `courses`, `payments`).

**O que colocar dentro de cada feature (ex: `features/auth/`):**
- **components/:** Componentes específicos desta feature (não genéricos).
- **services/:** Chamadas de API e integração com backend.
- **hooks/:** Lógicas React e gerenciamento de estado exclusivos.
- **types/:** Interfaces TS específicas do domínio da feature.
- **utils/:** Funções auxiliares locais.

**Regras:**
- Uma feature **não deve** acessar arquivos internos de outra feature diretamente. Comunique-se através de propriedades, estados globais ou arquivos públicos da feature.
- Mantenha o encapsulamento: se um código é usado por duas features distintas, ele deve ser movido para `shared/` ou `components/`.
