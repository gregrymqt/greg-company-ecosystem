using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.DTOs;

public record AutoRecurringDto(
    [property: JsonPropertyName("frequency")] int Frequency,
    [property: JsonPropertyName("frequency_type")] string FrequencyType,
    [property: JsonPropertyName("transaction_amount")] decimal TransactionAmount,
    [property: JsonPropertyName("currency_id")] string CurrencyId
);

public record CreatePlanDto(
    [property: JsonPropertyName("reason")]
    [Required(ErrorMessage = "The plan reason/name is required.")]
    [StringLength(256, ErrorMessage = "The reason must be up to 256 characters long.")]
        string? Reason,
    [property: JsonPropertyName("auto_recurring")]
    [Required(ErrorMessage = "Auto-recurring details are required.")]
        AutoRecurringDto? AutoRecurring,
    [property: JsonPropertyName("description")]
    [Required(ErrorMessage = "The description is required.")]
        string? Description
);

public record PlanDto(
    string PublicId,
    [Required] [StringLength(100)] string? Name,
    [Required] [StringLength(100)] string? Slug,
    [Required] [StringLength(50)] string? PriceDisplay,
    [Required] [StringLength(100)] string? BillingInfo,
    List<string> Features,
    bool IsRecommended,
    bool IsActive,
    int Frequency
);

public record PlanEditDto(
    string PublicId,
    string Name,
    decimal TransactionAmount,
    int Frequency,
    string FrequencyType,
    string? Description
);

public record PlanResponseDto(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("reason")] string? Reason,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("date_created")] DateTime DateCreated,
    [property: JsonPropertyName("external_reference")] string? ExternalPlanId,
    [property: JsonPropertyName("auto_recurring")] AutoRecurringDto? AutoRecurring
);

public record PlanSearchResponseDto(
    [property: JsonPropertyName("results")] List<PlanResponseDto> Results
);

public record UpdatePlanDto(
    [property: JsonPropertyName("reason")] string? Reason,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("auto_recurring")] AutoRecurringDto? AutoRecurring
);

public class PagedResultDto<T>
{
    public List<T> Items { get; set; }

    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public PagedResultDto(List<T> items, int currentPage, int pageSize, int totalCount)
    {
        Items = items;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
