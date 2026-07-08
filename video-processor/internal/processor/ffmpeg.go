package processor

import (
	"bufio"
	"context"
	"fmt"
	"log/slog"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"time"
	"video-processor/internal/storage"
)

type FfmpegProcessor struct {
	storage *storage.SupabaseStorage // Injeção do nosso serviço S3 do Supabase
}

func NewFfmpegProcessor(store *storage.SupabaseStorage) *FfmpegProcessor {
	return &FfmpegProcessor{storage: store}
}

// ProcessVideo coordena todo o ciclo de vida do processamento de mídia: Download -> Transcode -> Upload -> Cleanup
func (p *FfmpegProcessor) ProcessVideo(videoId, storageIdentifier, rawVideoPath string) (float64, error) {
	ctx, cancel := context.WithTimeout(context.Background(), 20*time.Minute) // Timeout de segurança por vídeo
	defer cancel()

	// 1. Faz o parse do 'rawVideoPath' (ex: "raw-videos/arquivo.mp4") para extrair o bucket e a chave
	parts := strings.SplitN(rawVideoPath, "/", 2)
	if len(parts) < 2 {
		return 0,fmt.Errorf("invalid raw video path format: %s", rawVideoPath)
	}
	bucket := parts[0]
	objectKey := parts[1]

	// 2. Cria uma diretoria temporária única na pasta local do SO para este Job específico
	tmpDir, err := os.MkdirTemp("", "video-proc-*")
	if err != nil {
		return 0,fmt.Errorf("failed to create sandbox directory: %w", err)
	}
	defer os.RemoveAll(tmpDir) // GARANTIA ARQUITETURAL: Deleta absolutamente tudo do disco ao finalizar (sucesso ou erro)

	localInputPath := filepath.Join(tmpDir, "input"+filepath.Ext(objectKey))
	localOutputDir := filepath.Join(tmpDir, "hls")
	if err := os.MkdirAll(localOutputDir, 0755); err != nil {
		return 0,fmt.Errorf("failed to create hls output directory: %w", err)
	}

	// 3. Executa o Download via Stream do arquivo bruto armazenado no Supabase
	err = p.storage.DownloadVideoToDisk(ctx, bucket, objectKey, localInputPath)
	if err != nil {
		return 0,fmt.Errorf("failed to download source video: %w", err)
	}

	// 4. Utiliza o ffprobe para extrair a duração exata do vídeo (essencial para o cálculo da percentagem)
	duration, err := p.getVideoDuration(ctx, localInputPath)
	if err != nil {
		slog.Warn("Could not parse video duration with ffprobe, progress tracking will be bypassed", "error", err)
	}

	// 5. Configura e dispara o FFmpeg via os/exec para segmentação HLS
	manifestPath := filepath.Join(localOutputDir, "manifest.m3u8")
	segmentPattern := filepath.Join(localOutputDir, "segment%03d.ts")

	args := []string{
		"-i", localInputPath,
		"-c:v", "libx264",
		"-c:a", "aac",
		"-hls_time", "10",
		"-hls_playlist_type", "vod",
		"-hls_segment_filename", segmentPattern,
		manifestPath,
	}

	cmd := exec.CommandContext(ctx, "ffmpeg", args...)

	// Captura o StderrPipe porque é por lá que o FFmpeg cospe os logs de renderização
	stderr, err := cmd.StderrPipe()
	if err != nil {
		return 0,fmt.Errorf("failed to allocate stderr pipe: %w", err)
	}

	if err := cmd.Start(); err != nil {
		return 0,fmt.Errorf("failed to start ffmpeg command: %w", err)
	}

	// Regex idêntica à utilizada no ecossistema C# para capturar a string 'time=00:00:00.00'
	timeRegex := regexp.MustCompile(`time=(\d{2}):(\d{2}):(\d{2})\.(\d{2})`)
	scanner := bufio.NewScanner(stderr)

	slog.Info("FFmpeg command executed. Transcoding started...", "video_id", videoId)

	// Lê a saída do FFmpeg linha por linha em tempo real
	for scanner.Scan() {
		line := scanner.Text()
		matches := timeRegex.FindStringSubmatch(line)
		
		if len(matches) == 5 && duration > 0 {
			hours, _ := strconv.Atoi(matches[1])
			minutes, _ := strconv.Atoi(matches[2])
			seconds, _ := strconv.Atoi(matches[3])

			totalSeconds := float64(hours*3600 + minutes*60 + seconds)
			percent := (totalSeconds / duration) * 100
			if percent > 100 {
				percent = 100
			}

			slog.Info("Transcoding video", 
				"video_id", videoId, 
				"progress", fmt.Sprintf("%d%%", int(percent)),
			)
			// NOTA: Este é o gatilho perfeito para o Passo 4 enviar mensagens de progresso parciais ao RabbitMQ
		}
	}

	if err := scanner.Err(); err != nil {
		slog.Warn("Error reading ffmpeg stderr", "video_id", videoId, "error", err)
	}

	// Aguarda a finalização do processo do SO
	if err := cmd.Wait(); err != nil {
		return 0,fmt.Errorf("ffmpeg process exited with error: %w", err)
	}

	slog.Info("FFmpeg successfully sliced the video into HLS chunks", "video_id", videoId)

	// 6. Faz o upload em lote de todos os pedaços .ts e do .m3u8 gerados para o bucket de processados
	// O caminho remoto organizado será: "processed-videos/{storageIdentifier}/hls/manifest.m3u8"
	remotePrefix := fmt.Sprintf("%s/hls", storageIdentifier)
	err = p.storage.UploadHLSFolder(ctx, "processed-videos", remotePrefix, localOutputDir)
	if err != nil {
		return 0,fmt.Errorf("failed to upload HLS directory to supabase: %w", err)
	}

	return duration, nil
}

// getVideoDuration executa o ffprobe nativo para inspecionar os segundos totais do arquivo em disco
func (p *FfmpegProcessor) getVideoDuration(ctx context.Context, inputPath string) (float64, error) {
	args := []string{
		"-v", "error",
		"-show_entries", "format=duration",
		"-of", "default=noprint_wrappers=1:nokey=1",
		inputPath,
	}

	cmd := exec.CommandContext(ctx, "ffprobe", args...)
	output, err := cmd.Output()
	if err != nil {
		return 0, err
	}

	durationStr := strings.TrimSpace(string(output))
	duration, err := strconv.ParseFloat(durationStr, 64)
	if err != nil {
		return 0, err
	}

	return duration, nil
}