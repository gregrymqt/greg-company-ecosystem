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
        public string Title { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public string Status { get; set; } = "open";
        public string? AssignedTo { get; set; }
        public List<SupportResponse> Responses { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
