import React from 'react';
import styles from '../styles/VideoPlayerFrame.module.scss';
import type { PlayerVideoDto } from '../../shared';

interface VideoPlayerFrameProps {
  video: PlayerVideoDto;
  onStart: () => void;
  isPlaying: boolean;
  videoRef: React.RefObject<HTMLVideoElement | null>;
}

export const VideoPlayerFrame: React.FC<VideoPlayerFrameProps> = ({
  video,
  onStart,
  isPlaying,
  videoRef
}) => {
  return (
    <div className={styles.playerWrapper}>
      <div className={styles.aspectRatioBox}>
        {isPlaying ? (
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
