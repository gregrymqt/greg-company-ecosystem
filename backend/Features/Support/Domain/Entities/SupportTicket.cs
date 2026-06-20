using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;

namespace MeuCrudCsharp.Features.Support.Domain.Entities
{
    public class SupportTicket : IMongoDocument
    {
        public static string CollectionName => "support_tickets";

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Context { get; set; } = null!;
        public string Explanation { get; set; } = null!;
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
