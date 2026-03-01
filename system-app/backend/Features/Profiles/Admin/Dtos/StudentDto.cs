﻿using System;
using System.ComponentModel.DataAnnotations;

namespace MeuCrudCsharp.Features.Profiles.Admin.Dtos;

public record StudentDto(
    [Required]
    string? Id,

    [Required]
    [StringLength(100)]
    string? Name,

    [Required]
    [EmailAddress]
    string? Email,

    string? SubscriptionStatus,

    string? PlanName,
    
    [Required]
    DateTime? RegistrationDate,
    
    string? SubscriptionId
);

public class PaginatedResult<T>
{
    public required List<T> Items { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public long TotalCount { get; set; }
}