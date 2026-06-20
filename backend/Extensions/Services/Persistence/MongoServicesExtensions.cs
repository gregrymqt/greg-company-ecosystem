using MongoDB.Driver;
using MeuCrudCsharp.Data;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class MongoServicesExtensions
{
    public static WebApplicationBuilder AddMongoPersistence(this WebApplicationBuilder builder)
    {
        // 1. Mapeia de forma segura as variáveis carregadas do seu .env
        var settings = new MongoDbSettings
        {
            ConnectionString = builder.Configuration.GetConnectionString("MongoConnection")
                ?? throw new InvalidOperationException("A string de conexão 'MongoConnection' não foi encontrada."),
            DatabaseName = builder.Configuration["MONGO_DATABASE_NAME"] ?? "GregCompanyMongo",
            WriteConcern = builder.Configuration["MONGO_WRITE_CONCERN"] ?? "Majority",
            Journal = !bool.TryParse(builder.Configuration["MONGO_JOURNAL"], out var journal) || journal
        };

        // Registrar o objeto de configurações como Singleton para uso no Contexto
        builder.Services.AddSingleton(settings);

        // 2. Registrar o IMongoClient como Singleton (Gerencia o pool de conexões internamente)
        builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));

        // 3. Registrar o seu MongoDbContext customizado como Scoped
        builder.Services.AddScoped<IMongoDbContext, MongoDbContext>();

        // 4. Bônus: Registrar o IMongoDatabase caso precise injetá-lo diretamente em algum repositório legado
        builder.Services.AddScoped<IMongoDatabase>(sp => sp.GetRequiredService<IMongoDbContext>().Database);

        return builder;
    }
}