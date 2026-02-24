FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test

WORKDIR /app

COPY Tests.csproj .
RUN dotnet restore

COPY . .

ENTRYPOINT ["dotnet", "test", "--no-restore", "--logger", "trx;LogFileName=test-results.trx", "--results-directory", "/app/TestResults"]
