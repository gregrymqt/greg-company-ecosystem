# 📁 infra/

**Objetivo:** Centralizar toda a orquestração e configurações de infraestrutura (Infra as Code) do ecossistema.

Este diretório contém os scripts e definições necessários para rodar a aplicação inteira em ambientes locais ou de produção.

**Arquivos Principais:**
- `docker-compose.yml`: A stack global em Docker. Sobe bancos de dados, mensageria (RabbitMQ), Nginx (Proxy), Backend, Worker e os bots, tudo interligado em uma única rede (`greg-network`).
- `nginx.conf`: O arquivo de configuração do API Gateway. Ele roteia o tráfego da porta 80 para os respectivos micro-frontends e microserviços (backend, go-worker, bot).
- `kind-config.yaml`: Configuração de cluster Kubernetes local usando o **KinD** (Kubernetes in Docker). Define os *nodes* virtuais para simular a nuvem.
- `up.sh`: Script utilitário em Shell para provisionamento rápido da infraestrutura ou para inicializar o cluster Kubernetes e carregar as imagens.

**Regras de Ouro:**
- **Paridade de Variáveis (Environment Drift):** Sempre que você criar uma variável de ambiente nova (como chaves de APIs ou rotas) nos serviços (`.env`), você **deve** refleti-la no `docker-compose.yml` (para ambiente Docker) e nos segredos do Kubernetes (pasta `manifests/`).
- Não coloque senhas hardcoded nesses arquivos. Use a interpolação de variáveis (`${VARIAVEL}`) apontando para o arquivo `.env` seguro.
