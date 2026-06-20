using MeuCrudCsharp.Features.Videos.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;

namespace MeuCrudCsharp.Features.Courses.Domain.Entities;

public class Course : IMongoDocument
{
    public static string CollectionName => "courses";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public Guid PublicId { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    [MongoIndex(Unique = true)]
    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Propriedade de navegação para a lista de vídeos que pertencem a este curso
    [BsonIgnore]
    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();
}
