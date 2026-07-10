# ========================================================
# Estágio 1: Build & Publish (SDK .NET 8 Completo)
# ========================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Desativa telemetria do .NET para otimizar o tempo de build em pipelines
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Otimização de Cache: Restaura as dependências NuGet isoladamente
COPY ["MeuCrudCsharp.csproj", "."]
RUN dotnet restore "MeuCrudCsharp.csproj"

# Copia todo o resto do código-fonte
COPY . .

# Executa o publish direto (o publish faz o build interno automaticamente)
RUN dotnet publish "MeuCrudCsharp.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:TypeScriptCompileBlocked=true

# ========================================================
# Estágio 2: Runtime (Imagem ASP.NET Otimizada e Segura)
# ========================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Configurações explícitas do ambiente de produção
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

# Copia os artefatos estritamente necessários gerados no estágio anterior
COPY --from=build /app/publish .

# Garante o princípio do privilégio mínimo usando o usuário nativo do .NET 8
USER app

EXPOSE 8080

ENTRYPOINT ["dotnet", "MeuCrudCsharp.dll"]