using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeuCrudCsharp.Features.About.Domain.Entities;

public class AboutSection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    // Pode ser Ãºtil para ordenar as seÃ§Ãµes na tela
    public int OrderIndex { get; set; }

    public string Title { get; set; } = string.Empty; // Ref: AboutSectionDto [cite: 20]

    public string Description { get; set; } = string.Empty; // Pode ser HTML ou Texto longo

    public string ImageUrl { get; set; } = string.Empty; // URL vinda do upload

    public string? FileId { get; set; } // FK opcional para rastrear o arquivo

    public string ImageAlt { get; set; } = string.Empty;
}


