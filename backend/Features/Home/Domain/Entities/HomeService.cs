using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;

namespace MeuCrudCsharp.Features.Home.Domain.Entities;

public class HomeServiceEntry : IMongoDocument
{
    public static string CollectionName => "home_services";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string IconClass { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ActionText { get; set; } = string.Empty;

    public string ActionUrl { get; set; } = string.Empty;
}

