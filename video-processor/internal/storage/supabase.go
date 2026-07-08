package storage

import (
	"context"
	"fmt"
	"io"
	"log/slog"
	"os"
	"path/filepath"
	"strings"

	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/credentials"
	"github.com/aws/aws-sdk-go-v2/service/s3"


	"video-processor/internal/config"
 	"github.com/aws/aws-sdk-go-v2/config"
)

type SupabaseStorage struct {
	s3Client *s3.Client
}

// NewSupabaseStorage inicializa o cliente S3 configurado para apontar para o Supabase
func NewSupabaseStorage(cfg *configenv.ConfigEnv) (*SupabaseStorage, error) {
	ctx := context.Background()

	// Carrega as credenciais estáticas do Supabase fornecidas pelas variáveis de ambiente
	awsCfg, err := config.LoadDefaultConfig(ctx,
		config.WithRegion("us-east-1"), // O Supabase ignora a região, mas o SDK exige um valor padrão
		config.WithCredentialsProvider(credentials.NewStaticCredentialsProvider(
			cfg.SupabaseAccessKeyId,
			cfg.SupabaseSecretAccessKey,
			"",
		)),
	)
	if err != nil {
		return nil, fmt.Errorf("unable to load SDK config: %w", err)
	}

	// Instancia o cliente S3 forçando o uso de PathStyle (exigido pelo Supabase Storage)
	s3Client := s3.NewFromConfig(awsCfg, func(o *s3.Options) {
		o.BaseEndpoint = aws.String(cfg.SupabaseS3URL)
		o.UsePathStyle = true
	})

	return &SupabaseStorage{s3Client: s3Client}, nil
}

// DownloadVideoToDisk baixa o vídeo original do bucket via stream e grava direto no disco local (ephemeral storage)
// Isso impede o estouro da memória RAM do container, mantendo o uso de memória em poucos megabytes
func (s *SupabaseStorage) DownloadVideoToDisk(ctx context.Context, bucket, objectKey, destLocalPath string) error {
	slog.Info("Starting streamed download from Supabase", "bucket", bucket, "key", objectKey)

	input := &s3.GetObjectInput{
		Bucket: aws.String(bucket),
		Key:    aws.String(objectKey),
	}

	result, err := s.s3Client.GetObject(ctx, input)
	if err != nil {
		return fmt.Errorf("failed to get object from supabase: %w", err)
	}
	defer result.Body.Close()

	// Cria o arquivo físico local onde o stream será despejado
	localFile, err := os.Create(destLocalPath)
	if err != nil {
		return fmt.Errorf("failed to create local temporary file: %w", err)
	}
	defer localFile.Close()

	// io.Copy faz o streaming do buffer de rede diretamente para o arquivo em disco em pedaços de 32KB.
	// A RAM do container nunca vai alocar o tamanho total do vídeo.
	bytesWritten, err := io.Copy(localFile, result.Body)
	if err != nil {
		return fmt.Errorf("failed to stream video to disk: %w", err)
	}

	slog.Info("Download completed successfully", "bytes_written", bytesWritten, "path", destLocalPath)
	return nil
}

// UploadHLSFolder varre o diretório local onde o FFmpeg gerou os pedaços (.ts e .m3u8) e faz o upload em lote
func (s *SupabaseStorage) UploadHLSFolder(ctx context.Context, bucket, remotePrefix, localDirPath string) error {
	slog.Info("Starting upload of HLS output folder", "bucket", bucket, "remote_prefix", remotePrefix)

	files, err := os.ReadDir(localDirPath)
	if err != nil {
		return fmt.Errorf("failed to read local HLS directory: %w", err)
	}

	for _, file := range files {
		if file.IsDir() {
			continue
		}

		localFilePath := filepath.Join(localDirPath, file.Name())
		// Monta o caminho remoto mantendo a estrutura da pasta (ex: storage_identifier/hls/segment001.ts)
		remoteKey := fmt.Sprintf("%s/%s", strings.TrimSuffix(remotePrefix, "/"), file.Name())

		err := s.uploadSingleFile(ctx, bucket, remoteKey, localFilePath)
		if err != nil {
			return fmt.Errorf("failed to upload file %s: %w", file.Name(), err)
		}
	}

	slog.Info("All HLS artifacts uploaded successfully to Supabase", "total_files", len(files))
	return nil
}

func (s *SupabaseStorage) uploadSingleFile(ctx context.Context, bucket, remoteKey, localFilePath string) error {
	file, err := os.Open(localFilePath)
	if err != nil {
		return fmt.Errorf("failed to open local file for upload: %w", err)
	}
	defer file.Close()

	// Determina o Content-Type correto baseado na extensão para o player de vídeo do front-end ler nativamente
	contentType := "application/octet-stream"
	if strings.HasSuffix(localFilePath, ".m3u8") {
		contentType = "application/x-mpegURL"
	} else if strings.HasSuffix(localFilePath, ".ts") {
		contentType = "video/MP2T"
	}

	slog.Debug("Uploading chunk", "key", remoteKey, "content_type", contentType)

	_, err = s.s3Client.PutObject(ctx, &s3.PutObjectInput{
		Bucket:      aws.String(bucket),
		Key:         aws.String(remoteKey),
		Body:        file,
		ContentType: aws.String(contentType),
	})

	return err
}