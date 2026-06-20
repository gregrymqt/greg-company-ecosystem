using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;
using MeuCrudCsharp.Data.Configuration.Attributes;
using MeuCrudCsharp.Data.Configuration.Interfaces;

namespace MeuCrudCsharp.Extensions.App.Initialization;

public static class MongoInitializationExtensions
{
    public static async Task InitializeMongoIndexesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetService<IMongoDatabase>();

        if (database == null)
            return;

        var documentTypes = typeof(IMongoDocument)
            .Assembly.GetTypes()
            .Where(p =>
                typeof(IMongoDocument).IsAssignableFrom(p)
                && p is { IsInterface: false, IsAbstract: false }
            );

        foreach (var docType in documentTypes)
        {
            var collectionNameProperty = docType.GetProperty(
                "CollectionName",
                BindingFlags.Public | BindingFlags.Static
            );
            var collectionName = collectionNameProperty?.GetValue(null) as string;

            if (string.IsNullOrEmpty(collectionName))
                continue;

            var collection = database.GetCollection<BsonDocument>(collectionName);
            var propertiesToIndex = docType
                .GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(MongoIndexAttribute)));

            foreach (var prop in propertiesToIndex)
            {
                var attribute = prop.GetCustomAttribute<MongoIndexAttribute>();
                var bsonElement =
                    prop.GetCustomAttribute<MongoDB.Bson.Serialization.Attributes.BsonElementAttribute>();
                var fieldName = bsonElement?.ElementName ?? prop.Name;

                try
                {
                    var indexKeys = attribute is { Descending: true }
                        ? Builders<BsonDocument>.IndexKeys.Descending(fieldName)
                        : Builders<BsonDocument>.IndexKeys.Ascending(fieldName);

                    var indexOptions = new CreateIndexOptions { Unique = attribute?.Unique };
                    var indexModel = new CreateIndexModel<BsonDocument>(indexKeys, indexOptions);

                    await collection.Indexes.CreateOneAsync(indexModel);
                    Console.WriteLine(
                        $"--> [Mongo] Index garantido em '{collectionName}': {fieldName}"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"--> [Mongo] Erro no index '{collectionName}': {ex.Message}"
                    );
                }
            }
        }
    }
}
