using System;
using System.ComponentModel.DataAnnotations;

namespace MeuCrudCsharp.Features.Shared.Domain.Entities;

public class OutboxEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool Processed { get; set; } = false;

    public string? Error { get; set; }
}
