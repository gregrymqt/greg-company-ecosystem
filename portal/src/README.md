# Portal / src

Este documento tem como objetivo servir como um guia para Inteligências Artificiais e Desenvolvedores sobre a estrutura de pastas e organização de código do diretório `src` do projeto Portal.

Esta é uma aplicação React (provavelmente usando Vite, baseada na presença de `main.tsx` e `App.tsx`).

## Estrutura de Diretórios

- `assets/`: Contém arquivos estáticos e recursos não baseados em código, como imagens, ícones, fontes, etc., que são importados diretamente pelos componentes.
- `components/`: Componentes React genéricos e reutilizáveis através de toda a aplicação (ex: botões, inputs, modais). Geralmente, são componentes "burros" (dumb/presentational components) que não possuem lógica de negócios complexa.
- `features/`: Organização baseada em funcionalidades (Feature-Sliced Design ou similar). Contém a lógica, componentes específicos, estado e serviços agrupados por contexto de negócios em vez de agrupados por tipo de arquivo.
- `pages/`: Componentes React que representam rotas/telas completas da aplicação. Ficam responsáveis por compor os componentes de `features` e `components` para montar a interface de uma URL específica.
- `routes/`: Configurações de roteamento da aplicação (ex: react-router-dom). Define quais `pages/` são renderizadas em quais URLs.
- `shared/`: Código, utilitários, hooks, constantes e recursos que são estritamente compartilhados globalmente por toda a aplicação e não pertencem a nenhuma `feature` específica.
- `styles/`: Arquivos de estilos globais (CSS/SCSS), configurações de temas, variáveis de estilo, etc.
- `types/`: Definições de tipos TypeScript globais, interfaces e enums que são usados em vários lugares do projeto.
- `utils/`: Funções utilitárias puras, formatadores de dados, e helpers genéricos que não têm estado e não estão acoplados ao React.

## Arquivos Principais

- `App.tsx`: O componente raiz da aplicação, geralmente onde os provedores de contexto (Context Providers) globais e as rotas principais são injetados.
- `main.tsx`: Ponto de entrada (entry point) da aplicação. É responsável por renderizar a árvore do React no DOM (normalmente usando `ReactDOM.createRoot`).
- `index.css`: Estilos CSS de nível superior importados diretamente no ponto de entrada.
- `App.css`: Estilos específicos do componente `App`.
- `declarations.d.ts`: Arquivo de declaração do TypeScript para fornecer tipagem a módulos não-TS (como imagens, CSS modules, etc.).

## Orientação para IAs

Ao atuar neste diretório:
1. **Mantenha a Coesão**: Se estiver criando uma nova funcionalidade, prefira encapsular tudo dentro de `features/` ao invés de espalhar pelos diretórios genéricos.
2. **Reutilização**: Procure em `components/` e `shared/` antes de criar novos componentes ou utilitários para evitar duplicação.
3. **Tipagem**: Mantenha o código rigorosamente tipado. Exporte interfaces ou tipos complexos e reutilizáveis para a pasta `types/` se não pertencerem exclusivamente a uma feature.
