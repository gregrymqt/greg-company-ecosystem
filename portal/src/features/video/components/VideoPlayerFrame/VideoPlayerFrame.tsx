import React, { useEffect } from 'react';
import styles from '../styles/VideoPlayerFrame.module.scss';
import type { PlayerVideoDto } from '../../../shared';
import { useVideoProgress } from '../../hooks/useVideoProgress';

interface VideoPlayerFrameProps {
  video: PlayerVideoDto;
  onStart: () => void;
  isPlaying: boolean;
  videoRef: React.RefObject<HTMLVideoElement | null>;
  onRefresh?: () => void;
}

export const VideoPlayerFrame: React.FC<VideoPlayerFrameProps> = ({
  video,
  onStart,
  isPlaying,
  videoRef,
  onRefresh
}) => {
  const isProcessing = video.status === 'Processing';
  const { progress, status: wsStatus } = useVideoProgress(isProcessing ? video.storageIdentifier : null);

  useEffect(() => {
    if (wsStatus === 'SUCCESS' && onRefresh) {
      onRefresh();
    }
  }, [wsStatus, onRefresh]);

  return (
    <div className={styles.playerWrapper}>
      <div className={styles.aspectRatioBox}>
        {isProcessing ? (
          <div className={styles.posterContainer} style={{
            display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
            height: '100%', backgroundColor: '#111', color: '#fff', textAlign: 'center'
          }}>
            <h3 style={{ marginBottom: '8px' }}>Este vídeo está sendo otimizado para a sua conexão...</h3>
            <p style={{ marginBottom: '16px' }}>Progresso: {Math.floor(progress)}%</p>
            <div style={{ width: '60%', height: '8px', backgroundColor: '#333', borderRadius: '4px' }}>
              <div style={{ width: `${progress}%`, height: '100%', backgroundColor: '#007bff', borderRadius: '4px', transition: 'width 0.3s ease' }} />
            </div>
          </div>
        ) : isPlaying ? (
          <video
            ref={videoRef}
            className={styles.nativePlayer}
            controls
            autoPlay
            playsInline
          />
        ) : (
          <div
            className={styles.posterContainer}
            onClick={onStart}
            style={{ backgroundImage: `url(${video.thumbnailUrl})` }}
          >
            <div className={styles.playOverlay}>
              <button className={styles.bigPlayButton} aria-label="Reproduzir vídeo">
                ▶
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
