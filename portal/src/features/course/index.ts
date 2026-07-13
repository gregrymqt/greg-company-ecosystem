/**
 * Barrel exports para Course Feature (Portal)
 */

// Types
export type { CourseRowUI, VideoCardUI, CourseFilters, CourseDto } from './types/course.types';

// Services
export { publicCourseService } from './services/publicCourse.service';

// Hooks
export { usePublicCourses } from './hooks/usePublicCourses';

// Components
export { CourseSkeleton } from './components/CourseSkeleton/CourseSkeleton';
export { CourseRow } from './components/CourseRow/CourseRow';
export { CourseDetailModal } from './components/CourseDetailModal/CourseDetailModal';
export { VideoCard } from './components/VideoCard/VideoCard';

// Pages
export { CourseFeed } from './pages/CourseFeed';
