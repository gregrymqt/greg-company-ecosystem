# 📁 shared/

**Objetivo:** Código genérico, utilitários, configurações e constantes que pertencem a **toda a aplicação** e são consumidos por múltiplas *features*.

**Exemplos do que colocar aqui:**
- **constants/:** Valores imutáveis globais (`API_BASE_URL`, máscaras de input, chaves do LocalStorage).
- **hooks/:** Custom hooks genéricos que não pertencem a um contexto de negócio (ex: `useWindowSize`, `useDebounce`, `useLocalStorage`).
- **contexts/:** React Contexts que abrangem o app todo (ex: `ThemeContext`, `AuthGlobalContext`).
- **libs/ ou config/:** Configurações de bibliotecas terceiras (ex: instância global do `axios`, configuração do `i18n`, init do `firebase`).

**Regras:**
- **NÃO** coloque código de domínio/negócio aqui. (Por exemplo, `useCoursesList` deve ir para `features/courses/hooks/`, mas `usePagination` genérico vai para `shared/hooks/`).
- O código do `shared/` não deve importar nada de dentro da pasta `features/` para não criar acoplamento bidirecional. O `shared` é sempre a base.
