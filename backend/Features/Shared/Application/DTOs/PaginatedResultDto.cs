namespace MeuCrudCsharp.Features.Shared.Application.DTOs;

public record PaginatedResultDto<T>(List<T> Items, int TotalCount, int Page, int PageSize);
