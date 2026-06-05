# AI Agent Instructions - DevOps & Infrastructure Specialist

## Architecture Pattern: Immutable Infrastructure & Service Orchestration

- **Ambientes Isolados por Containers**: Todo o ecossistema deve ser empacotado em imagens de containers leves e imutáveis, garantindo reprodutibilidade idêntica do desenvolvimento à produção.
- **Orquestração Declarativa**: Toda a pilha de serviços e infraestrutura local (bancos de dados, filas, caches, instâncias de aplicação) deve ser descrita e gerenciada via arquivos de composição declarativos (*Compose/Manifests*).
- **Desacoplamento de Ambiente**: A aplicação deve se comportar de forma agnóstica ao ambiente de execução, configurando seu fluxo lógico estritamente a partir de variáveis injetadas em tempo de execução.

## Critical Conventions

- **Segurança Pró-Ativa (Zero Trust)**: Proibição absoluta de segredos, chaves privadas, tokens ou strings de conexão fixadas no código-fonte. Todo dado sensível deve vir de arquivos de ambiente isolados (`.env`) ou gerenciadores de segredos.
- **Rede Isolada por Nomes**: Serviços containerizados devem se comunicar internamente utilizando nomes de serviços lógicos fornecidos pelo ecossistema de rede da orquestração, nunca endereços locais estáticos (*localhost*).
- **Emissão Estruturada de Logs**: Configurar as aplicações para emitirem logs padronizados e estruturados simultaneamente no console (para captura de coletores) e em arquivos físicos com rotação diária automática.
- **Fail-Fast Pipelines**: Pipelines de integração contínua (CI) devem interromper imediatamente qualquer processo de entrega (CD) caso os testes unitários ou etapas de análise estática de código (*linting*) falhem.

## Common Pitfalls

- **Ignorar Variáveis Ausentes**: Deixar de validar a existência de variáveis de ambiente obrigatórias no início da inicialização da aplicação, gerando falhas catastróficas em tempo de execução.
- **Imagens de Container Infladas**: Utilizar imagens base pesadas e não otimizadas, aumentando o tempo de build e a superfície de vulnerabilidades.
- **Persistência Volátil**: Esquecer de mapear volumes persistentes para containers de bancos de dados ou logs, ocasionando perda de dados críticos em reinicializações do serviço.

## Adding New Features

1. **Mapeamento de Requisitos**: Identifique se a nova feature exige uma nova dependência de infraestrutura, serviço de background ou novas variáveis de ambiente.
2. **Atualização da Orquestração**: Adicione ou ajuste as definições do serviço nos arquivos de composição e configure os volumes e redes necessários.
3. **Blindagem de Configurações**: Adicione os novos parâmetros necessários no arquivo de exemplo de ambiente (`.env.example`) sem expor dados reais de produção.
4. **Integração na Pipeline**: Garanta que as novas etapas de teste ou novas dependências sejam corretamente provisionadas nos scripts automatizados da esteira de CI/CD.
5. **Validação de Logs e Telemetria**: Monitore o comportamento do container nos terminais e verifique se o fluxo de logs estruturados está registrando a inicialização com sucesso.
