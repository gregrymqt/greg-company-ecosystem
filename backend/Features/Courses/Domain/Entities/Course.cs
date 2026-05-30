using MeuCrudCsharp.Features.Videos.Domain.Entities;

namespace MeuCrudCsharp.Features.Courses.Domain.Entities;

public class Course
{
    public int Id { get; set; }

    public Guid PublicId { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Propriedade de navegação para a lista de vídeos que pertencem a este curso
    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();
}
