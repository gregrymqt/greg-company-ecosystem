namespace MeuCrudCsharp.Features.Auth.Application.DTOs
{
    public class UserSessionDto
    {
        public Guid PublicId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
        public List<string> Tenants { get; set; } = [];

        // Novos campos booleanos leves (substituindo os objetos completos)
        public bool HasActiveSubscription { get; set; }
        public bool HasPaymentHistory { get; set; }
        
        // Flag para admin de cursos
        public bool IsCourseAdmin { get; set; }
    }
}
