using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeuCrudCsharp.Features.Home.Domain.Entities;

public class HomeHero
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string? FileId { get; set; }

    public string ActionText { get; set; } = string.Empty;

    public string ActionUrl { get; set; } = string.Empty;
}



