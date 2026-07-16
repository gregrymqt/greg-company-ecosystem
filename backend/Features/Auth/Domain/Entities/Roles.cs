using Microsoft.AspNetCore.Identity;

namespace MeuCrudCsharp.Features.Auth.Domain.Entities;

public class Roles : IdentityRole<Guid>
{
    public Roles() : base() { }
    public Roles(string roleName) : base(roleName) { }
}
