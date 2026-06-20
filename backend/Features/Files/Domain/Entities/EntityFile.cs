using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeuCrudCsharp.Features.Files.Domain.Entities;

public class EntityFile
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public required string NomeArquivo { get; set; } // Ex: "guid_relatorio.pdf"
    public required string CaminhoRelativo { get; set; } // Ex: "uploads/Financeiro/guid_relatorio.pdf"
    public required string ContentType { get; set; } // Ex: "application/pdf", "image/png"
    public required string FeatureCategoria { get; set; } // Ex: "Financeiro", "Perfil", "VideosAula"
    public long TamanhoBytes { get; set; } // Útil para validações
}

