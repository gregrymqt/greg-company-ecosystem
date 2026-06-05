using MongoDB.Driver;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class MongoServicesExtensions
{
    public static WebApplicationBuilder AddMongoPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMongoClient>(_ =>
        {
            var mongoConnString = builder.Configuration.GetConnectionString("MongoConnection");
            return new MongoClient(mongoConnString);
        });

        builder.Services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();

            // 💡 BÔNUS: Aproveite para puxar o nome do banco dinamicamente do seu .env também!
            var dbName = builder.Configuration["MONGO_DATABASE_NAME"] ?? "MeuCrudSupportDb";

            return client.GetDatabase(dbName);
        });

        return builder;
    }
}
