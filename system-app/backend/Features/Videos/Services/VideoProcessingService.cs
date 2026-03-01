﻿using System.Globalization;
using System.Text.RegularExpressions;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Features.Videos.Interfaces;
using MeuCrudCsharp.Models;
using Microsoft.Extensions.Options;

namespace MeuCrudCsharp.Features.Videos.Services;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly ILogger<VideoProcessingService> _logger;
    private readonly IVideoRepository _videoRepository;
    private readonly IFileRepository _fileRepository;
    private readonly IVideoNotificationService _videoNotificationService;
    private readonly IProcessRunnerService _processRunnerService;
    private readonly FFmpegSettings _ffmpegSettings;
    private readonly IWebHostEnvironment _env;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly Regex FfmpegProgressRegex = new(
        @"time=(\d{2}):(\d{2}):(\d{2})\.(\d{2})",
        RegexOptions.Compiled
    );

    public VideoProcessingService(
        ILogger<VideoProcessingService> logger,
        IVideoRepository videoRepository,
        IFileRepository fileRepository,
        IVideoNotificationService videoNotificationService,
        IProcessRunnerService processRunnerService,
        IOptions<FFmpegSettings> ffmpegSettings,
        IWebHostEnvironment env,
        IUnitOfWork unitOfWork
    )
    {
        _logger = logger;
        _videoRepository = videoRepository;
        _fileRepository = fileRepository;
        _videoNotificationService = videoNotificationService;
        _processRunnerService = processRunnerService;
        _ffmpegSettings = ffmpegSettings.Value;
        _env = env;
        _unitOfWork = unitOfWork;
    }

    public async Task ProcessVideoToHlsAsync(int videoId, int fileId)
    {
        Video? video = null;
        var groupName = "";

        try
        {
            video = await _videoRepository.GetByIdAsync(videoId);
            var originalFile = await _fileRepository.GetByIdAsync(fileId);

            if (video == null || originalFile == null)
                throw new ResourceNotFoundException(
                    "Vídeo ou Arquivo original não encontrados no banco."
                );

            groupName = video.StorageIdentifier;

            var inputFilePath = Path.Combine(_env.WebRootPath, originalFile.CaminhoRelativo);

            var videoDirectory = Path.Combine(
                _env.WebRootPath,
                "uploads",
                "Videos",
                video.StorageIdentifier
            );

            var hlsOutputDirectory = Path.Combine(videoDirectory, "hls");

            if (!Directory.Exists(hlsOutputDirectory))
                Directory.CreateDirectory(hlsOutputDirectory);

            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException(
                    $"Arquivo físico não encontrado em: {inputFilePath}"
                );

            await _videoNotificationService.SendProgressUpdate(
                groupName,
                "Iniciando análise...",
                0
            );

            var duration = await GetVideoDurationAsync(inputFilePath);
            video.Duration = duration;
            await _videoRepository.UpdateAsync(video);
            
            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation(
                "Duração do vídeo {VideoId} detectada: {Duration}",
                videoId,
                duration
            );

            var manifestPath = Path.Combine(hlsOutputDirectory, "manifest.m3u8");
            var segmentPattern = Path.Combine(hlsOutputDirectory, "segment%03d.ts");

            var arguments =
                $"-i \"{inputFilePath}\" -c:v libx264 -c:a aac -hls_time 10 -hls_playlist_type vod -hls_segment_filename \"{segmentPattern}\" \"{manifestPath}\"";

            Task OnProgress(string rawOutput)
            {
                var progressPercent = ParseFfmpegProgress(rawOutput, duration.TotalSeconds);
                return progressPercent.HasValue
                    ? _videoNotificationService.SendProgressUpdate(groupName, "Convertendo...", progressPercent.Value)
                    : Task.CompletedTask;
            }

            await _videoNotificationService.SendProgressUpdate(
                groupName,
                "Convertendo vídeo...",
                5
            );

            await _processRunnerService.RunProcessWithProgressAsync(
                _ffmpegSettings.FfmpegPath,
                arguments,
                OnProgress
            );

            await _videoRepository.UpdateStatusAsync(videoId, VideoStatus.Available);
            
            await _unitOfWork.CommitAsync();

            await _videoNotificationService.SendProgressUpdate(
                groupName,
                "Processamento concluído!",
                100,
                isComplete: true
            );

            _logger.LogInformation(
                "Vídeo {Id} ({StorageIdentifier}) processado com sucesso e disponível para reprodução.",
                videoId,
                video.StorageIdentifier
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no processamento do vídeo ID {VideoId}", videoId);

            if (video == null) throw;
            
            await _videoRepository.UpdateStatusAsync(videoId, VideoStatus.Error);
            
            await _unitOfWork.CommitAsync();
            
            await _videoNotificationService.SendProgressUpdate(
                groupName,
                $"Erro: {ex.Message}",
                0,
                isError: true
            );

            throw;
        }
    }

    private async Task<TimeSpan> GetVideoDurationAsync(string inputFilePath)
    {
        var arguments =
            $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputFilePath}\"";

        var result = await _processRunnerService.RunProcessAndGetOutputAsync(
            _ffmpegSettings.FfprobePath,
            arguments
        );

        if (
            double.TryParse(
                result.StandardOutput,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var seconds
            )
        )
        {
            return TimeSpan.FromSeconds(seconds);
        }

        _logger.LogWarning(
            "Falha ao ler duração. Saída: {Output} Erro: {Error}",
            result.StandardOutput,
            result.StandardError
        );
        return TimeSpan.Zero;
    }

    private int? ParseFfmpegProgress(string ffmpegLine, double totalDurationSeconds)
    {
        if (totalDurationSeconds <= 0)
            return null;

        var match = FfmpegProgressRegex.Match(ffmpegLine);
        if (!match.Success) return null;
        var hours = int.Parse(match.Groups[1].Value);
        var minutes = int.Parse(match.Groups[2].Value);
        var seconds = int.Parse(match.Groups[3].Value);
        var milliseconds = int.Parse(match.Groups[4].Value);

        var processedTime = new TimeSpan(0, hours, minutes, seconds, milliseconds * 10);
        var progress = (int)(processedTime.TotalSeconds / totalDurationSeconds * 100);

        return Math.Min(99, progress);

    }
}