using System.Collections.Generic;
using System.Text.Json.Serialization;
using MeuCrudCsharp.Features.Files.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace MeuCrudCsharp.Features.Home.Application.DTOs;

public class HomeContentDto
{
    [JsonPropertyName("hero")]
    public List<HeroSlideDto> Hero { get; set; } = new();

    [JsonPropertyName("services")]
    public List<ServiceDto> Services { get; set; } = new();
}

public class HeroSlideDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string CtaText { get; set; } = string.Empty;
    public string CtaLink { get; set; } = string.Empty;
    public string? Audience { get; set; }
    public int Order { get; set; }
}

public class ServiceDto
{
    public Guid Id { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CtaText { get; set; } = string.Empty;
    public string CtaLink { get; set; } = string.Empty;
    public string? Audience { get; set; }
    public int Order { get; set; }
}

public class CreateUpdateHeroDto : BaseUploadDto
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string CtaText { get; set; } = string.Empty;
    public string CtaLink { get; set; } = string.Empty;
    public string? Audience { get; set; }
    public int Order { get; set; }
}

public class CreateUpdateServiceDto
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CtaText { get; set; } = string.Empty;
    public string CtaLink { get; set; } = string.Empty;
    public string? Audience { get; set; }
    public int Order { get; set; }
}



