# 📁 Data/

**Objetivo:** Abstrações centrais de acesso a dados (Repositórios Base e Contextos).

Embora cada *Feature* tenha seus próprios repositórios específicos (ex: `CourseRepository`), é nesta pasta que as interfaces fundamentais e a conexão direta com o banco residem.

**O que colocar aqui:**
- `MongoDbContext.cs`: Configuração da conexão nativa com o **MongoDB** e lógicas de transação (ex: *ClientSession* para transações ACID).
- Interfaces base como `IRepository<T>` ou definições do Padrão *Outbox*.

**Regras:**
- Alterações no contexto do MongoDB devem ser feitas com extrema cautela, especialmente em configurações de concorrência ou mapeamento global (ex: `BsonClassMap`).
