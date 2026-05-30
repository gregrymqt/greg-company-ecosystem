# AI Agent Instructions - Core Architecture & Features Specialist

## Architectural Pattern: Clean Architecture with Vertical Slice
- **Features-First Organization**: Todas as regras de negócio e componentes devem ser organizados estritamente sob diretórios baseados em contextos de negócio (Features), e não por tipo técnico de arquivo.
- **Autocontenção**: Cada feature deve encapsular seus próprios pontos de entrada (padrão controller/handler), serviços, repositórios de dados, contratos (interfaces), mapeadores e objetos de transferência de dados (DTOs).
- **Isolamento do Domínio**: A lógica de negócio central deve residir em uma camada pura (Core/Domain), totalmente desacoplada e agnóstica de detalhes externos como bancos de dados, frameworks ou interfaces de usuário.

## Critical Conventions
- **Princípios SOLID**: Impor responsabilidade única a nível de classe e método. Interfaces estáveis devem ditar a comunicação entre diferentes módulos por meio de injeção de dependência.
- **Validação Antecipada**: Dados de entrada devem ser completamente validados (padrão Request/DTO) antes de atingirem qualquer fluxo ou manipulador de regra de negócio.
- **Desacoplamento de Contratos**: Nunca exponha entidades ou estruturas internas de persistência diretamente nos pontos de entrada da API. Use mappers para expor apenas ViewModels/Responses.

## Common Pitfalls
- **Acoplamento Circular**: Evitar dependências bidirecionais entre componentes. Utilize inversão de dependência via contratos.
- **Vazamento de Escopo**: Nunca misture consultas brutas de persistência ou manipulação visual diretamente dentro da camada de domínio ou serviços de aplicação.
- **Contratos Fracos**: Passar tipos primitivos genéricos quando um objeto de valor (Value Object) ou DTO tipado deveria garantir a segurança da assinatura.

## Adding New Features
1. **Mapeamento de Impacto**: Analise a árvore de pastas atual e determine o escopo exato da nova funcionalidade sob o diretório global de features.
2. **Criação da Estrutura**: Modele a nova pasta da feature contendo suas subpastas lógicas isoladas.
3. **Definição de Contratos**: Escreva as interfaces que guiarão a comunicação da feature com serviços externos ou infraestrutura.
4. **Implementação da Regra**: Codifique a lógica de negócio isolada, garantindo conformidade com o princípio de responsabilidade única.
5. **Exposição e Registro**: Crie o ponto de entrada (API/Endpoint) consumindo o serviço via injeção de dependência e certifique-se de que o sistema de varredura ou registro automático descubra o novo componente.