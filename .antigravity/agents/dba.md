# AI Agent Instructions - Database & Data Engineering Specialist

## Architecture Pattern: Structured Abstraction & Hybrid Data Slices

- **Acesso Isolado a Dados**: A persistência deve ser inteiramente mediada por uma camada de acesso dedicada (Repositories/Data Context), impedindo o acoplamento de queries brutas nas camadas superiores.
- **Evolução Declarativa Incremental**: Toda e qualquer alteração na estrutura do esquema deve ser realizada exclusivamente por meio de arquivos de migração (*Migrations*) sequenciais e versionados.
- **Persistência Poliglota**: Isolar dados puramente relacionais e transacionais em motores ACID, e dados flexíveis, logs ou documentos desestruturados em motores NoSQL de alta leitura.

## Critical Conventions

- **Integridade Referencial**: Garantir chaves primárias e estrangeiras bem definidas, além de restrições (*constraints*) severas para manter a consistência mútua dos dados.
- **Estratégia de Indexação**: Projetar índices estratégicos para chaves de busca frequentes, evitando leituras completas de tabelas (*full table scans*), monitorando o impacto em operações de escrita.
- **Retrocompatibilidade de Esquema**: Alterações em bancos de dados de produção nunca devem quebrar a versão atual da aplicação. Remocões ou renomeações de colunas/campos devem ser feitas em etapas incrementais seguras.
- **Gerenciamento de Cache**: Identificar gargalos de leitura repetitiva em dados estáticos ou de alta demanda e aplicar estratégias de invalidação de cache temporário.

## Common Pitfalls

- **Problema do N+1**: Executar consultas em loops de repetição em vez de realizar junções e carregamentos agressivos (*eager loading*) na primeira consulta.
- **Locks de Tabela**: Executar atualizações ou remoções em massa sem paginação ou sem chaves indexadas, travando linhas ou tabelas inteiras em produção.
- **Queries Opacas**: Construir subconsultas excessivamente aninhadas ou junções (*joins*) redundantes que degradam o plano de execução do banco.

## Adding New Features

1. **Modelagem de Entidades**: Desenhe o modelo de dados focado na feature, escolhendo o motor de persistência adequado (relacional vs. documento) com base na natureza do dado.
2. **Geração de Migração**: Crie o arquivo de migração incremental a partir do diretório de dados correto e revise o código SQL/declarativo gerado.
3. **Implementação de Repositórios**: Desenvolva os métodos de acesso aos dados, blindando o restante da aplicação contra especificidades do banco.
4. **Otimização de Consultas**: Avalie o plano de execução da query, verifique a necessidade de índices e configure o comportamento do cache se necessário.
