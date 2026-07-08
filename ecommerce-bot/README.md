# E-commerce Bot 🛒🤖

Um sistema de automação completo para extração e enriquecimento de dados de produtos de e-commerce. O projeto utiliza web scraping para coletar informações brutas de produtos e integrações com LLM (OpenAI) para reescrever descrições com foco em persuasão de vendas (copywriting).

## 🏗️ Arquitetura do Sistema

O projeto foi construído focando em processamento assíncrono e arquitetura desacoplada:

- **ScraperWorker:** Navega por páginas de um e-commerce alvo, faz o parse do HTML e extrai produtos, salvando-os no banco de dados com status `raw`.
- **ProcessorWorker:** Atua de forma independente buscando itens na fila do banco (status `raw`) e enviando para a API da OpenAI. Se bem sucedido, o status muda para `processing` e depois `processed`. Em caso de falha, muda para `error`.
- **Database (MongoDB):** Armazena o estado dos produtos. Conta com índices de performance em `status` e `sku` (único) garantindo integridade e agilidade na fila de processamento.
- **Modelagem Pydantic:** Validação rigorosa dos dados na entrada da aplicação, garantindo URLs consistentes e tipagens corretas.

## 🚀 Como Rodar o Projeto Localmente

### 1. Pré-requisitos
- Python 3.10+ instalado.
- Docker e Docker Compose (para subir o MongoDB).

### 2. Configuração do Banco de Dados
Na raiz do projeto, inicie o contêiner do MongoDB usando o Docker Compose:
```bash
docker-compose up -d
```

### 3. Variáveis de Ambiente (.env)
Crie um arquivo `.env` na raiz do projeto (se ainda não existir) baseado no modelo abaixo:
```env
# Chave da API da OpenAI
OPENAI_API_KEY=sk-sua-chave-aqui

# Conexão com o banco (O padrão para o docker local é esse)
MONGO_URI=mongodb://localhost:27017

# URL do e-commerce alvo
SCRAPER_BASE_URL=https://site-do-fornecedor.com/produtos
```

### 4. Ambiente Virtual e Dependências
Crie um ambiente virtual (`venv`) para isolar as bibliotecas:
```bash
# Criar o ambiente
python -m venv venv

# Ativar no Windows (PowerShell)
.\venv\Scripts\activate

# Instalar dependências
pip install -r requirements.txt
```

### 5. Iniciando a Aplicação
Com o banco rodando e as dependências instaladas, basta executar o ponto de entrada principal:
```bash
python -m app.main
```
Os logs começarão a aparecer no terminal demonstrando a extração e enriquecimento dos itens de forma paralela.
