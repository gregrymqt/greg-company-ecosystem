# 📁 styles/

**Objetivo:** Arquivos responsáveis pela estilização global e design system básico da aplicação.

**Exemplos do que colocar aqui:**
- `global.css` ou `index.css`: Estilos fundamentais de _reset_ do navegador (body, html, box-sizing).
- Variáveis CSS ou tokens de design (ex: `colors.css`, `typography.css`).
- Configurações globais caso use bibliotecas como TailwindCSS, Styled-Components (themes) ou SCSS (mixins e variáveis globais).

**Regras:**
- **NÃO** coloque CSS específicos de um componente de botão ou de uma página aqui (estes devem estar encapsulados junto ao seu componente, através de CSS Modules ou Styled Components).
- Mantenha esta pasta apenas para o que afeta o app inteiramente ou dita a identidade visual geral.
