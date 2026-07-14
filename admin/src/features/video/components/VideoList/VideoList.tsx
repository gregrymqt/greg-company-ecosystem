import React from "react";
export interface VideoListProps {
  videos: VideoDto[];
  isLoading: boolean;
  onEdit: (video: VideoDto) => void;
  onDelete: (id: string) => void;
  onWatch: (video: VideoDto) => void;
  onNewClick: () => void;
  
}
import styles from './VideoList.module.scss';
import { ActionMenu } from "@/components/ActionMenu/ActionMenu";
import { type TableColumn, Table } from "@/components/Table/Table";
import type { VideoDto, VideoStatus } from "../..";

export const VideoList: React.FC<VideoListProps> = ({
  videos,
  isLoading,
  onEdit,
  onDelete,
  onWatch,
  onNewClick
}) => {
  
  // Função auxiliar para status simples (Agora será utilizada)
  const getStatusBadge = (status: VideoStatus) => {
    switch (status) {
      case 'Available':
        return (
          <span className={`${styles.statusBadge} ${styles.available}`}>
            Disponível
          </span>
        );
      case 'Processing':
        return (
          <span className={`${styles.statusBadge} ${styles.processing}`}>
            Processando
          </span>
        );
      case 'Error':
        return (
          <span className={`${styles.statusBadge} ${styles.error}`}>Erro</span>
        );
      default:
        return <span>-</span>;
    }
  };

  const columns: TableColumn<VideoDto>[] = [
    {
      header: "Preview",
      width: "80px",
      render: (video: VideoDto) => (
        <button
          className={styles.playBtn}
          onClick={() => onWatch(video)}
          title="Assistir Vídeo"
        >
          <i className="fas fa-play"></i>
        </button>
      ),
    },
    { header: "Título", accessor: "title", width: "30%" },
    {
      header: "Curso",
      width: "25%",
      render: (video: VideoDto) => video.courseName || "Sem Curso",
    },
    { header: "Duração", accessor: "duration", width: "15%" },
    {
      header: "Status",
      width: "20%",
      render: (video: VideoDto) => {
        // Usa a função auxiliar para retornar o status em texto/badge
        return getStatusBadge(video.status);
      },
    },
    {
      header: "Ações",
      width: "10%",
      render: (video: VideoDto) => (
        <ActionMenu
          onEdit={() => onEdit(video)}
          onDelete={() => onDelete(video.id)}
        />
      ),
    },
  ];

  return (
    <div className={styles.listContainer}>
      <div className={styles.header}>
        <h2>Gerenciar Vídeos</h2>
        <button className={styles.createBtn} onClick={onNewClick}>
          <i className="fas fa-plus"></i> Novo Vídeo
        </button>
      </div>

      <Table
        data={videos}
        columns={columns}
        keyExtractor={(v: VideoDto) => v.id}
        isLoading={isLoading}
        emptyMessage="Nenhum vídeo encontrado."
      />
    </div>
  );
};
