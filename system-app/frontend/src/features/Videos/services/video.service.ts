import { ApiService } from "@/shared/services/api.service";
import type { Video } from "@/types/models";
import type {
  VideoFilters,
  PaginatedResponse,
  CreateVideoParams,
  UpdateVideoParams,
  CreateVideoPayload,
} from "@/features/Videos/types/video-manager.types";

const BASE_ENDPOINT = "/admin/videos";

export const VideoService = {
  // GET
  getAll: async (filters: VideoFilters): Promise<PaginatedResponse<Video>> => {
    const params = new URLSearchParams({
      page: filters.page.toString(),
      pageSize: filters.pageSize.toString(),
    });

    return await ApiService.get<PaginatedResponse<Video>>(
      `${BASE_ENDPOINT}?${params.toString()}`
    );
  },

  // POST - Criação (Pode conter Vídeo Grande + Thumbnail)
  // VideoService.ts

  create: async (data: CreateVideoParams): Promise<Video> => {
    const { videoFile, thumbnailFile, ...textDto } = data;

    // Aqui substituímos o <Video, any> pelo novo tipo CreateVideoPayload
    return await ApiService.postWithFile<Video, CreateVideoPayload>(
      BASE_ENDPOINT,
      { ...textDto, ThumbnailFile: thumbnailFile }, 
      videoFile, 
      "File" // Chave esperada pelo BaseUploadDto no C#
    );
  },

  // PUT - Atualização
  update: async (id: string, data: UpdateVideoParams): Promise<Video> => {
    const { thumbnailFile, ...dto } = data;

    // Na atualização, geralmente só atualizamos a thumbnail ou dados de texto
    const file = thumbnailFile || null;

    return await ApiService.putWithFile<Video, typeof dto>(
      `${BASE_ENDPOINT}/${id}`,
      dto,
      file,
      "thumbnailFile" // Se for só a thumb, podemos usar o nome específico se o backend exigir
    );
  },

  // DELETE
  delete: async (id: string): Promise<void> => {
    await ApiService.delete(`${BASE_ENDPOINT}/${id}`);
  },
};
