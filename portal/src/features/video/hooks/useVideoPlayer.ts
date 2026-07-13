/**
 * Hook para player de vídeo (Public)
 * Gerencia HLS streaming e metadados do vídeo
 */

import { useState, useEffect, useRef, useCallback } from 'react';
import Hls from 'hls.js';
import { publicVideoService } from '../services/video-public.service';
import { useVideoProgress } from './useVideoProgress';
import type { PlayerVideoDto, VideoDto } from '..';

export const useVideoPlayer = (publicId: string | undefined) => {
  const [video, setVideo] = useState<PlayerVideoDto | null>(null);
  const [internalData, setInternalData] = useState<VideoDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isPlaying, setIsPlaying] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const videoRef = useRef<HTMLVideoElement>(null);
  const hlsRef = useRef<Hls | null>(null);

  const { progress: conversionProgress, status: conversionStatus } = useVideoProgress(internalData?.storageIdentifier || null);

  // Busca metadados do vídeo
  const refreshVideo = useCallback(async () => {
    if (!publicId) return;
    setIsLoading(true);
    try {
      const data = await publicVideoService.getById(publicId);
      setInternalData(data);
      setVideo(publicVideoService.toPlayerDto(data));
    } catch (err) {
      setError('Erro ao carregar informações do vídeo.');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  }, [publicId]);

  useEffect(() => {
    refreshVideo();
  }, [refreshVideo]);

  // Lógica de streaming HLS
  useEffect(() => {
    const isAvailable = internalData?.status === 'Available' || conversionStatus === 'SUCCESS';
    if (!isPlaying || !videoRef.current || !internalData || !isAvailable) return;

    const videoElement = videoRef.current;
    const manifestUrl = publicVideoService.getManifestUrl(internalData.storageIdentifier);

    // HLS.js para navegadores (exceto Safari)
    if (Hls.isSupported()) {
      const hls = new Hls({
        startLevel: -1, // Auto quality
        capLevelToPlayerSize: true
      });

      hls.loadSource(manifestUrl);
      hls.attachMedia(videoElement);

      hls.on(Hls.Events.MANIFEST_PARSED, () => {
        videoElement.play().catch(e => console.log('Autoplay bloqueado', e));
      });

      hls.on(Hls.Events.ERROR, (_event, data) => {
        if (data.fatal) {
          console.error('Erro fatal HLS:', data);
          setError('Erro ao reproduzir o vídeo.');
        }
      });

      hlsRef.current = hls;
    }
    // Fallback para Safari/iOS (suporte nativo HLS)
    else if (videoElement.canPlayType('application/vnd.apple.mpegurl')) {
      videoElement.src = manifestUrl;
      videoElement.addEventListener('loadedmetadata', () => {
        videoElement.play();
      });
    }

    return () => {
      if (hlsRef.current) {
        hlsRef.current.destroy();
      }
    };
  }, [isPlaying, internalData, conversionStatus]);

  const handleStart = useCallback(() => {
    setIsPlaying(true);

    if (internalData) {
      const courseIdToSave = internalData.courseId || internalData.id;
      const cacheKey = `@GregCompany:LastWatched:Course:${courseIdToSave}`;
      const historyPayload = {
        videoId: internalData.id,
        title: internalData.title,
        thumbnailUrl: internalData.thumbnailUrl,
        durationFormatted: internalData.duration,
        courseId: internalData.courseId,
        courseName: internalData.courseName,
        lastWatchedAt: new Date().toISOString()
      };
      localStorage.setItem(cacheKey, JSON.stringify(historyPayload));
    }
  }, [internalData]);

  const handlePause = useCallback(() => {
    if (videoRef.current) {
      videoRef.current.pause();
    }
  }, []);

  const handleResume = useCallback(() => {
    if (videoRef.current) {
      videoRef.current.play();
    }
  }, []);

  return {
    video,
    isLoading,
    isPlaying,
    error,
    videoRef,
    handleStart,
    handlePause,
    handleResume,
    refreshVideo,
    conversionProgress,
    conversionStatus
  };
};
