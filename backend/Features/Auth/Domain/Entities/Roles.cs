using Microsoft.AspNetCore.Identity;

namespace MeuCrudCsharp.Features.Auth.Domain.Entities;

public class Roles : AspNetCore.Identity.Mongo.Model.MongoRole<string>
{
    public Roles() : base() { }
    public Roles(string roleName) : base(roleName) { }
}

