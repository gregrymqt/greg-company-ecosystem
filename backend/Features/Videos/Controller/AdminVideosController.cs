using Hangfire;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Files.Attributes;
using MeuCrudCsharp.Features.Videos.DTOs;
using MeuCrudCsharp.Features.Videos.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.Videos.Controller
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/videos")]
    public class AdminVideosController : ApiControllerBase
    {
        private readonly IAdminVideoService _videoService;

        public AdminVideosController(IAdminVideoService videoService)
        {
            _videoService = videoService;
        }

        [HttpPost]
        [AllowLargeFile(3072)]
        public async Task<IActionResult> CreateVideo([FromForm] CreateVideoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _videoService.HandleVideoUploadAsync(dto);

                if (result == null)
                {
                    return Ok(new { message = $"Chunk {dto.ChunkIndex} do vídeo recebido." });
                }

                return CreatedAtAction(nameof(GetAllVideos), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao criar vídeo.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVideos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            var result = await _videoService.GetAllVideosAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [AllowLargeFile(1024)]
        public async Task<IActionResult> UpdateVideo(Guid id, [FromForm] UpdateVideoDto dto)
        {
            try
            {
                var updated = await _videoService.UpdateVideoAsync(id, dto);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao atualizar vídeo.");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteVideo(Guid id)
        {
            try
            {
                await _videoService.DeleteVideoAsync(id);
                return Ok(new { Message = "Vídeo e arquivos deletados com sucesso." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao deletar vídeo.");
            }
        }
    }
}
