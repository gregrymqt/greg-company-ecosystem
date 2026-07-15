using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeuCrudCsharp.Features.Courses.Domain.Entities;

public class Lesson
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public Guid PublicId { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public int Order { get; set; }

    // Reference to an existing Video.
    public Guid VideoPublicId { get; set; } 
    public string VideoTitle { get; set; } = string.Empty;
}
