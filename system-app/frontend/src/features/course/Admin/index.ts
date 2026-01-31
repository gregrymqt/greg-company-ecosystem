/**
 * Barrel exports para Admin Course Feature
 */

export { adminCourseService } from './services/course.service';
export { useAdminCourses } from './hooks/useAdminCourses';
export { CourseForm } from './components/CourseForm';
export { CourseList } from './components/CourseList';
export type { AdminTab, CourseFormData, CourseListProps, CourseFormProps } from './types/admin-course.types';
