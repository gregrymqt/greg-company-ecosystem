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
            return client.GetDatabase("MeuCrudSupportDb");
        });

        return builder;
    }
}