package processor

import "log/slog"

type FfmpegProcessor struct{}

func NewFfmpegProcessor() *FfmpegProcessor {
	return &FfmpegProcessor{}
}

func (p *FfmpegProcessor) ProcessVideo(videoId, storageIdentifier, rawVideoPath string) error {
	slog.Info("FFMPEG processing started (stub)", 
		"video_id", videoId, 
		"storage_identifier", storageIdentifier, 
		"raw_video_path", rawVideoPath)
	return nil
}
