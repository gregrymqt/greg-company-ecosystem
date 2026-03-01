using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Videos.Controller
{
    [Route("/api/videos")]
    public class VideosController : ApiControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<VideosController> _logger;

        public VideosController(
            ApiDbContext context,
            IWebHostEnvironment env,
            ILogger<VideosController> logger
        )
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        [HttpGet("{storageIdentifier}/manifest.m3u8")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetManifest(string storageIdentifier)
        {
            var videoExists = await _context.Videos.AnyAsync(v =>
                v.StorageIdentifier == storageIdentifier && v.Status == VideoStatus.Available
            );

            if (!videoExists)
            {
                return NotFound("Video not found or is not yet available.");
            }

            var manifestPath = Path.Combine(
                _env.WebRootPath,
                "uploads",
                "Videos",
                storageIdentifier,
                "hls",
                "manifest.m3u8"
            );

            if (!System.IO.File.Exists(manifestPath))
            {
                _logger.LogError($"Manifest não encontrado em: {manifestPath}");
                return StatusCode(500, "Erro de processamento: Manifest não encontrado.");
            }

            return PhysicalFile(manifestPath, "application/vnd.apple.mpegurl");
        }

        [HttpGet("{storageIdentifier}/hls/{segmentName}")]
        public IActionResult GetVideoSegment(string storageIdentifier, string segmentName)
        {
            var segmentPath = Path.Combine(
                _env.WebRootPath,
                "uploads",
                "Videos",
                storageIdentifier,
                "hls",
                segmentName
            );

            if (!System.IO.File.Exists(segmentPath))
            {
                return NotFound("Video segment not found.");
            }

            return PhysicalFile(segmentPath, "video/mp2t");
        }
    }
}
