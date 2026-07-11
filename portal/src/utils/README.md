# 📁 utils/

**Objetivo:** Funções utilitárias puras (pure functions) e lógicas independentes que não têm ligação com o framework (React) nem com o estado.

**Exemplos do que colocar aqui:**
- `formatDate.ts`: Transforma strings ISO em "dd/mm/aaaa".
- `currencyFormatter.ts`: Transforma números em "R$ 10,00".
- `validators.ts`: Funções para validar CPF, e-mail, senhas, etc.

**Regras:**
- As funções aqui devem ser **puras**: se você passar os mesmos argumentos, elas sempre retornarão os mesmos valores e não causarão efeitos colaterais.
- **Não** chame Hooks do React (ex: `useState`, `useEffect`) dentro de um arquivo `utils`. Para isso, utilize a pasta `shared/hooks/`.
- Mantenha os arquivos pequenos e focados em operações de strings, matemática, arrays, objetos ou formatações.
