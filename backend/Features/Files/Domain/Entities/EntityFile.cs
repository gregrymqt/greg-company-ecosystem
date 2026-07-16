using System.ComponentModel.DataAnnotations;

namespace MeuCrudCsharp.Features.Files.Domain.Entities;

public class EntityFile
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string NomeArquivo { get; set; }
    public required string CaminhoRelativo { get; set; }
    public required string ContentType { get; set; }
    public required string FeatureCategoria { get; set; }
    public long TamanhoBytes { get; set; }
}
