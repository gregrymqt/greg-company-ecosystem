import React, { useState, useEffect, useMemo } from "react";

// Styles
import styles from "./AdminVideoManager.module.scss";
import { useCourses } from "@/features/course/hooks/useCourses";
import { Sidebar } from "@/components/SideBar";
import type { SidebarItem } from "@/components/SideBar/types/sidebar.types";
import { AlertService } from "@/shared/services/alert.service";
import { VideoForm } from "../components/VideoForm/VideoForm";
import { VideoList } from "../components/VideoList/VideoList";
import { useAdminVideos } from "../hooks/useAdminVideos";
import type { VideoDto, VideoFormData } from "../types/video.types";

type VideoTab = "list" | "form" | "player";

const VideoPlayer: React.FC<{ video: VideoDto | null, onBack: () => void }> = ({ onBack }) => (
  <div style={{ padding: '20px', color: 'white' }}>
    <h3>Visualizador Temporariamente Indisponível</h3>
    <button onClick={onBack} style={{ padding: '10px' }}>Voltar</button>
  </div>
);



export const AdminVideoManager: React.FC = () => {
  // 1. Instanciando os Hooks de Dados
  const {
    videos,
    loading: loadingVideos,
    fetchVideos,
    createVideo,
    updateVideo,
    deleteVideo,
  } = useAdminVideos();

  // Precisamos dos cursos para preencher o <select> do formulário
  const { courses, fetchCourses: refreshCourses } = useCourses();

  // Converter CourseRowUI para Course para compatibilidade com VideoForm
  const coursesForForm = useMemo<{ id: number; name: string; publicId: string; description: string; videos: any[] }[]>(() => {
    return courses.map((courseRow: any) => ({
      id: courseRow.id,
      publicId: String(courseRow.id), // CourseRowUI não tem publicId, usando id como fallback
      name: courseRow.categoryName,
      description: '', // CourseRowUI não tem description
      videos: [] // Não precisamos dos videos aqui
    }));
  }, [courses]);

  // 2. Estados de UI (Controle de Abas e Seleção)
  const [activeTab, setActiveTab] = useState<VideoTab>("list");
  const [selectedVideo, setSelectedVideo] = useState<VideoDto | null>(null);
  const [videoToWatch, setVideoToWatch] = useState<VideoDto | null>(null);

  // 3. Carregar dados iniciais
  useEffect(() => {
    fetchVideos();
    refreshCourses();
  }, [fetchVideos]);

  // Itens da Sidebar
  const sidebarItems: SidebarItem[] = [
    { id: "list", label: "Visualizar Dados", icon: "fas fa-list" },
    { id: "form", label: "Formulário", icon: "fas fa-plus-circle" },
    { id: "player", label: "Visualizar Vídeo", icon: "fas fa-play-circle" },
  ];

  // --- Handlers ---

  const handleTabChange = (item: SidebarItem) => {
    const tabId = item.id as VideoTab;

    if (tabId === "list" || tabId === "form") {
      setSelectedVideo(null);
    }
    setActiveTab(tabId);
  };

  const handleEdit = (video: VideoDto) => {
    setSelectedVideo(video);
    setActiveTab("form");
  };

  const handleWatch = (video: VideoDto) => {
    setVideoToWatch(video);
    setActiveTab("player");
  };

  const handleDelete = async (id: string) => {
    // O hook já gerencia o AlertService.confirm internamente
    const success = await deleteVideo(id);
    if (success) {
      fetchVideos(); // Garante sincronia
    }
  };

  const handleFormSubmit = async (data: VideoFormData) => {
    let success = false;

    // 1. Extraindo os Arquivos corretamente (sem usar 'any')
    const thumbnailFile =
      data.thumbnailFile && data.thumbnailFile.length > 0
        ? data.thumbnailFile[0]
        : undefined;

    // [CORREÇÃO] Agora o TypeScript reconhece 'videoFile' direto no objeto 'data'
    const videoFile =
      data.videoFile && data.videoFile.length > 0
        ? data.videoFile[0]
        : undefined;

    if (selectedVideo) {
      // --- UPDATE (Edição) ---
      // Na edição, geralmente não obrigamos reenviar o vídeo, apenas atualizamos dados
      success = await updateVideo(selectedVideo.id, {
        title: data.title,
        description: data.description,
        thumbnailFile: thumbnailFile,
      });
    } else {
      // --- CREATE (Criação) ---

      // [EXPLICAÇÃO DA LÓGICA]
      // Esta validação garante que o usuário enviou ALGUMA fonte de vídeo.
      if (!videoFile) {
        AlertService.error(
          "Atenção",
          "Para criar um vídeo, você deve fazer upload de um arquivo."
        );
        return; // Interrompe a função aqui, não chama o backend
      }

      // Se passou na validação, envia para o hook
      success = await createVideo({
        title: data.title,
        description: data.description,
        courseId: Number(data.courseId),
        videoFile: videoFile as File, // Casting seguro pois validamos acima
        thumbnailFile: thumbnailFile,
      });
    }

    if (success) {
      setActiveTab("list");
      setSelectedVideo(null);
      fetchVideos();
    }
  };

  return (
    <div className={styles.adminContainer}>
      <Sidebar
        items={sidebarItems}
        activeItemId={activeTab}
        onItemClick={handleTabChange}
        logo={<h3 style={{ color: "#fff", padding: "0 20px" }}>Admin Panel</h3>}
      />

      <main className={styles.contentArea}>
        <div className={styles.contentWrapper}>
          {activeTab === "list" && (
            <VideoList
              videos={videos?.items || []}
              isLoading={loadingVideos}
              onEdit={handleEdit}
              onDelete={handleDelete}
              onWatch={handleWatch}
              onNewClick={() => {
                setSelectedVideo(null);
                setActiveTab("form");
              }}
            />
          )}

          {activeTab === "form" && (
            <VideoForm
              initialData={selectedVideo || undefined}
              courses={coursesForForm} // Usando cursos convertidos
              isLoading={loadingVideos}
              onSubmit={handleFormSubmit}
              onCancel={() => setActiveTab("list")}
            />
          )}

          {activeTab === "player" && (
            <VideoPlayer
              video={videoToWatch}
              onBack={() => setActiveTab("list")}
            />
          )}
        </div>
      </main>
    </div>
  );
};
