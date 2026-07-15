using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MeuCrudCsharp.Features.Videos.Domain.Entities;

namespace MeuCrudCsharp.Features.Courses.Domain.Entities;

public class Module
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public Guid PublicId { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<Lesson> Lessons { get; set; } = new();
}
