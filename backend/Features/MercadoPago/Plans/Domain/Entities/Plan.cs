using System.ComponentModel.DataAnnotations;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities
{
    public enum PlanFrequencyType
    {
        Days,
        Months,
    }

    public class Plan
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PublicId { get; set; } = Guid.NewGuid();

        public string ExternalPlanId { get; set; } = string.Empty;

        public required string Name { get; set; }

        public string? Description { get; set; }

        public string Category { get; set; } = "course";

        public decimal TransactionAmount { get; set; }

        public string CurrencyId { get; set; } = "BRL";

        public int FrequencyInterval { get; set; }

        public PlanFrequencyType FrequencyType { get; set; }

        public bool IsActive { get; set; } = false;

        public List<string> IncludedCourseIds { get; set; } = new();

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    }
}
