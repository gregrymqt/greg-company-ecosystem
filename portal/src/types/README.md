# 📁 types/

**Objetivo:** Agrupar interfaces e declarações de tipos globais do TypeScript.

**Exemplos do que colocar aqui:**
- Interfaces de entidades que são usadas em toda a aplicação (ex: `User.d.ts`, `Session.d.ts`).
- Tipos genéricos de respostas de API ou erros comuns (`ApiResponse.ts`).

**Regras:**
- **NÃO** inclua interfaces que só são usadas em uma única *feature* aqui. (Por exemplo, `interface CourseDetails` deve ficar em `features/courses/types/`).
- Use esta pasta somente se o tipo de dado for transitar livremente por diversos arquivos não relacionados, justificando a sua globalidade.
