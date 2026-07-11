# 📁 Extensions/

**Objetivo:** Modularizar a inicialização do programa (`Program.cs`) através de métodos de extensão (`IServiceCollection` e `IApplicationBuilder`).

**O que colocar aqui:**
- `ServiceCollectionExtensions.cs`: Adição de Swagger, JWT, políticas de CORS, MongoDB, e auto-registro de dependências (Scrutor).
- `ApplicationBuilderExtensions.cs`: Configuração de Middlewares, mapeamento de Endpoints e tratamento global de erros (Exception Handlers).

**Regras:**
- O arquivo `Program.cs` deve permanecer limpo. Qualquer nova dependência global volumosa (ex: integração com um novo provedor de cache) deve virar um método de extensão nesta pasta.
