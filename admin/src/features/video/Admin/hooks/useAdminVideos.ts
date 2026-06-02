/**
 * Hook para gerenciamento administrativo de vídeos
 * CRUD completo + paginação
 */

import { useState, useCallback } from 'react';
import { adminVideoService } from '../services/video-admin.service';
import type {
  VideoDto,
  CreateVideoDto,
  UpdateVideoDto,
  PaginatedVideoResult,
  VideoFilters
} from '../../shared';
import { AlertService } from '@/shared/services/alert.service';

const DEFAULT_FILTERS: VideoFilters = {
  page: 1,
  pageSize: 10
};

export const useAdminVideos = () => {
  const [loading, setLoading] = useState(false);
  const [videos, setVideos] = useState<PaginatedVideoResult | null>(null);
  const [filters, setFilters] = useState<VideoFilters>(DEFAULT_FILTERS);

  /**
   * Busca vídeos com paginação
   */
  const fetchVideos = useCallback(async (customFilters?: Partial<VideoFilters>) => {
    const appliedFilters = { ...filters, ...customFilters };
    setLoading(true);

    try {
      const response = await adminVideoService.getAll(appliedFilters);
      setVideos(response);
      setFilters(appliedFilters);
    } catch (err) {
      const errorMessage = (err as Error).message || 'Erro ao carregar vídeos.';
      AlertService.error('Erro', errorMessage);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  /**
   * Cria novo vídeo com upload
   */
  const createVideo = async (data: CreateVideoDto): Promise<boolean> => {
    setLoading(true);

    try {
      await adminVideoService.create(data);

      await AlertService.success(
        'Upload Iniciado!',
        'O vídeo foi enviado e está sendo processado. Ele aparecerá na lista em breve.'
      );

      // Recarrega lista
      await fetchVideos({ page: 1 });
      return true;
    } catch (err) {
      const errorMessage = (err as Error).message || 'Falha no upload do vídeo.';
      AlertService.error('Erro no Upload', errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  };

  /**
   * Atualiza vídeo existente
   */
  const updateVideo = async (id: string, data: UpdateVideoDto): Promise<boolean> => {
    setLoading(true);

    try {
      await adminVideoService.update(id, data);

      AlertService.success('Sucesso', 'Informações do vídeo atualizadas.');

      // Recarrega lista
      await fetchVideos();
      return true;
    } catch (err) {
      const errorMessage = (err as Error).message || 'Erro ao atualizar.';
      AlertService.error('Erro', errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  };

  /**
   * Deleta vídeo
   */
  const deleteVideo = async (id: string): Promise<boolean> => {
    const { isConfirmed } = await AlertService.confirm(
      'Excluir Vídeo?',
      'Isso removerá o vídeo, a thumbnail e todos os arquivos de streaming permanentemente.'
    );

    if (!isConfirmed) return false;

    setLoading(true);

    try {
      await adminVideoService.delete(id);

      AlertService.success('Deletado', 'Vídeo removido com sucesso.');

      // Atualiza lista localmente
      if (videos) {
        setVideos({
          ...videos,
          items: videos.items.filter((v: VideoDto) => v.id !== id),
          totalCount: videos.totalCount - 1
        });
      }

      return true;
    } catch (err) {
      const errorMessage = (err as Error).message || 'Erro ao deletar vídeo.';
      AlertService.error('Erro', errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  };

  /**
   * Atualiza filtros
   */
  const updateFilters = useCallback((newFilters: Partial<VideoFilters>) => {
    setFilters((prev: VideoFilters) => ({ ...prev, ...newFilters }));
  }, []);

  return {
    videos,
    loading,
    filters,
    fetchVideos,
    createVideo,
    updateVideo,
    deleteVideo,
    updateFilters
  };
};
