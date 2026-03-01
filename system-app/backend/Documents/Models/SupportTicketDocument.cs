using MeuCrudCsharp.Documents.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeuCrudCsharp.Documents.Models
{
    public class SupportTicketDocument : IMongoDocument
    {
        public static string CollectionName => "support_tickets";

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        public required string UserId { get; set; }

        [BsonElement("context")]
        public required string Context { get; set; }

        [BsonElement("explanation")]
        public required string Explanation { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Open";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
