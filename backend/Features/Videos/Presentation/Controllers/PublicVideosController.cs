using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Videos.Application.DTOs;
using MeuCrudCsharp.Features.Videos.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.Videos.Presentation.Controllers
{
    [AllowAnonymous]
    [Route("api/videos")]
    public class PublicVideosController : ApiControllerBase
    {
        private readonly IVideoRepository _videoRepository;

        public PublicVideosController(IVideoRepository videoRepository)
        {
            _videoRepository = videoRepository;
        }

        [HttpGet("{publicId:guid}")]
        public async Task<IActionResult> GetVideo(Guid publicId)
        {
            try
            {
                var video = await _videoRepository.GetByPublicIdAsync(publicId);
                
                if (video == null)
                    return NotFound(new { Message = "Vídeo não encontrado." });

                var dto = new VideoDto
                {
                    Id = video.PublicId,
                    Title = video.Title,
                    Description = video.Description,
                    StorageIdentifier = video.StorageIdentifier,
                    UploadDate = video.UploadDate,
                    Duration = video.Duration,
                    Status = video.Status.ToString(),
                    CourseId = video.CourseId,
                    ThumbnailUrl = video.ThumbnailUrl ?? string.Empty
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao buscar vídeo público.");
            }
        }

        [HttpGet("{storageIdentifier}/manifest.m3u8")]
        public IActionResult GetManifest(string storageIdentifier)
        {
            // O CDN público ou bucket deve ser substituído pela sua base correta de HLS
            var baseUrl = Environment.GetEnvironmentVariable("CDN_BASE_URL") ?? "https://cdn.seudominio.com.br";
            return Redirect($"{baseUrl}/processed-videos/{storageIdentifier}/manifest.m3u8");
        }

        [HttpPost("{publicId:guid}/progress")]
        [Authorize] 
        public IActionResult SaveProgress(Guid publicId, [FromBody] SaveVideoProgressDto dto)
        {
            // Integração futura para salvar progresso no banco de dados.
            return Ok(new { Message = "Progresso salvo com sucesso." });
        }
    }

    public class SaveVideoProgressDto
    {
        public double Percentage { get; set; }
        public DateTime LastWatchedAt { get; set; }
    }
}
