using System;

namespace MeuCrudCsharp.Features.Auth.Domain.Interfaces;

public interface IUserRoleRepository
{
    Task<List<string>> GetRolesByUserIdAsync(string userId);
}
