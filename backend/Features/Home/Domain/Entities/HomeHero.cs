using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;

namespace MeuCrudCsharp.Features.Home.Domain.Entities;

public class HomeHero : IMongoDocument
{
    public static string CollectionName => "home_heroes";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string? FileId { get; set; }

    public string CtaText { get; set; } = string.Empty;

    public string CtaLink { get; set; } = string.Empty;

    public string? Audience { get; set; }

    public int Order { get; set; }
}



