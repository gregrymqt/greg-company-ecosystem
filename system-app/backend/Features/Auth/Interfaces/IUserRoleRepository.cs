using System;

namespace MeuCrudCsharp.Features.Auth.Interfaces;

public interface IUserRoleRepository
{
    Task<List<string>> GetRolesByUserIdAsync(string userId);
}
