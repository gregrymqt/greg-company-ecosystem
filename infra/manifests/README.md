# 📁 infra/manifests/

**Objetivo:** Armazenar as configurações e definições declarativas do Kubernetes (arquivos YAML).

Enquanto o `docker-compose.yml` serve primariamente para o ambiente local, esta pasta garante a orquestração oficial para a Nuvem e ambientes de produção distribuídos.

**Arquivos Típicos Esperados Aqui:**
- `apps-infra.yaml`: Deployments, Services, Ingress e ConfigMaps das aplicações do ecossistema (Backend, Frontends, Workers, Bots).
- `greg-secrets.yaml`: Arquivo de Secret Definition para injeção de senhas criptografadas ou tokens (ex: `OPENAI_API_KEY`, `AES_MASTER_KEY`). 
- `volumes.yaml`: PersistentVolume e PersistentVolumeClaims para armazenamentos persistentes do MongoDB ou RabbitMQ.

**Regras e Diretrizes para IAs e Devs:**
- **Nunca insira segredos em texto puro não-encodados em Base64** nos manifestos oficiais de Secrets (K8s exige que estejam encodados). 
- O arquivo `greg-secrets.yaml` tem prioridade de leitura de variáveis nos containers instanciados via cluster.
- Se você adicionar um microserviço novo ao ecossistema, você **deve** criar o Deployment e o Service correspondentes neste diretório para que ele fique visível na rede do cluster Kubernetes.
