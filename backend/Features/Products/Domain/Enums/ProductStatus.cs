using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MeuCrudCsharp.Features.Products.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductStatus
{
    [EnumMember(Value = "Raw")]
    Raw,
    [EnumMember(Value = "Processing")]
    Processing,
    [EnumMember(Value = "Processed")]
    Processed,
    [EnumMember(Value = "Failed")]
    Failed,
    [EnumMember(Value = "Exported")]
    Exported
}
