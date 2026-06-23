using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeuCrudCsharp.Features.Shared.Domain.Entities;

public class OutboxEvent
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("eventType")]
    public string EventType { get; set; } = string.Empty;

    [BsonElement("payload")]
    public string Payload { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("processed")]
    public bool Processed { get; set; } = false;

    [BsonElement("error")]
    public string? Error { get; set; }
}
