/**
 * Tipos específicos da área administrativa de cursos
 * Estende os tipos compartilhados com interfaces específicas de UI do Admin
 */

import type { CourseDto, CreateCourseData, UpdateCourseData } from '@/features/course/shared/types/course.types';

// Define as abas de navegação da Sidebar interna
export type AdminTab = 'list' | 'form';

/**
 * Interface específica para o Formulário
 * Estende o CreateCourseData com campos extras de UI
 */
export interface CourseFormData extends CreateCourseData {
  // Campo opcional para upload de imagem de capa futuramente
  thumbnail?: FileList;
}

/**
 * Props para o componente de listagem
 */
export interface CourseListProps {
  courses: CourseDto[];
  isLoading: boolean;
  onEdit: (course: CourseDto) => void;
  onDelete: (id: string) => void;
  onNewClick: () => void;
}

/**
 * Props para o componente de formulário
 */
export interface CourseFormProps {
  initialData?: CourseDto | null;  // Null se for criar, Objeto se for editar
  isLoading: boolean;
  onSubmit: (data: CourseFormData) => Promise<void>;
  onCancel: () => void;
}
