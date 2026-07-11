# 📁 Properties/

**Objetivo:** Configurações de compilação e *launch* (inicialização) do projeto .NET.

**Arquivos:**
- `launchSettings.json`: Define os perfis de como a aplicação sobe quando rodamos via IDE (Visual Studio, Rider) ou `dotnet run`. Configura as portas locais (ex: http://localhost:5000), variáveis de ambiente de dev e URLs.

**Regras:**
- Arquivos desta pasta não vão para o Docker/Kubernetes de produção. Eles são estritamente para o ambiente de desenvolvimento local e conforto dos desenvolvedores.
