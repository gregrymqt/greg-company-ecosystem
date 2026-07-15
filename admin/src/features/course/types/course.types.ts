/**
 * Tipagem compartilhada de Cursos - Alinhada com CourseDto.cs do Backend
 * 
 * Esta é a fonte única de verdade (Single Source of Truth) para tipos de cursos.
 * Tanto Admin quanto Public devem importar daqui.
 */

// Interface que espelha o VideoDto do backend
export interface VideoDto {
  publicId: string;
  title: string;
  thumbnailUrl?: string;
  duration?: {
    totalSeconds: number;
  };
  courseId: number;
}

export interface LessonDto {
  publicId: string;
  title: string;
  order: number;
  videoPublicId: string;
  videoTitle: string;
}

export interface ModuleDto {
  publicId: string;
  title: string;
  order: number;
  lessons: LessonDto[];
}

/**
 * Interface baseada em CourseDto.cs
 * Representa um curso retornado pela API
 */
export interface CourseDto {
  publicId: string;  // Guid no backend
  name: string;
  description: string;
  price: number;
  isPublished: boolean;
  thumbnailUrl?: string;
  modules?: ModuleDto[];  // Opcional na listagem, presente no detalhe
}

export interface CreateLessonData {
  title: string;
  order: number;
  videoPublicId: string;
  videoTitle: string;
}

export interface CreateModuleData {
  title: string;
  order: number;
  lessons: CreateLessonData[];
}

/**
 * Dados para criar um novo curso (CreateUpdateCourseDto do backend)
 */
export interface CreateCourseData {
  name: string;
  description?: string;
  price: number;
  isPublished: boolean;
  thumbnailUrl?: string;
  modules?: CreateModuleData[];
}

/**
 * Dados para atualizar um curso existente (CreateUpdateCourseDto do backend)
 */
export interface UpdateCourseData {
  name: string;
  description?: string;
  price: number;
  isPublished: boolean;
  thumbnailUrl?: string;
  modules?: CreateModuleData[];
}

/**
 * Interface genérica de paginação (alinhada com PaginatedResultDto do backend)
 */
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Filtros para listagem de cursos (usado em queries)
 */
export interface CourseFilters {
  pageNumber: number;
  pageSize: number;
  name?: string;  // Para busca
}

/**
 * Modelos de UI - Transformações específicas para apresentação
 */

// Modelo visual único para o Card de Vídeo
export interface VideoCardUI {
  id: string;              // Mapeado do publicId
  title: string;
  thumbnailUrl: string;
  durationFormatted: string; // Já formatado (ex: "12m")
  courseId: number;
  isNew?: boolean;         // Badge "Novo"
}

// Modelo visual para a Fileira (Categoria)
export interface CourseRowUI {
  id: string;              // publicId
  categoryName: string;    // Nome do curso
  videos: VideoCardUI[];
}
