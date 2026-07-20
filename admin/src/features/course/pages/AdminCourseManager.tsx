import React, { useState, useEffect } from 'react';
import { Sidebar } from '@/components/SideBar';
import type { SidebarItem } from '@/components/SideBar/types/sidebar.types';
import type { CourseDto } from '@/features/course/types/course.types';

// Sub-components, Estilos e Types
import { CourseList } from '@/features/course/components/CourseList/CourseList';
import { CourseForm } from '@/features/course/components/CourseForm/CourseForm';
import styles from './AdminCourseManager.module.scss';
import type { CourseFormData, AdminTab } from '@/features/course/types/admin-course.types';

// Hook refatorado
import { useAdminCourses } from '@/features/course';

export const AdminCourseManager: React.FC = () => {
  // Hook gerencia o estado dos cursos e loading
  const { 
    courses, 
    loading, 
    fetchCourses, 
    createCourse, 
    updateCourse, 
    deleteCourse 
  } = useAdminCourses();

  const [activeTab, setActiveTab] = useState<AdminTab>('list');
  const [editingCourse, setEditingCourse] = useState<CourseDto | null>(null);

  // [NOVO] Carrega os cursos ao montar o componente
  useEffect(() => {
    fetchCourses();
  }, [fetchCourses]);

  const sidebarItems: SidebarItem[] = [
    { id: 'list', label: 'Visualizar Dados', icon: 'fas fa-list' },
    { id: 'form', label: 'Formulário', icon: 'fas fa-plus-circle' }
  ];

  // --- Handlers ---

  const handleTabChange = (item: SidebarItem) => {
    setEditingCourse(null);
    setActiveTab(item.id as AdminTab);
  };

  const handleCreateNew = () => {
    setEditingCourse(null);
    setActiveTab('form');
  };

  const handleEdit = (course: CourseDto) => {
    setEditingCourse(course);
    setActiveTab('form');
  };

  // [CORREÇÃO] Usando a função do hook (o AlertService já está dentro do hook)
  const handleDelete = async (id: string) => {
    await deleteCourse(id);
  };

  // [CORREÇÃO] Lógica de Submit conectada ao Hook
  const handleFormSubmit = async (data: CourseFormData) => {
    let success = false;

    if (editingCourse) {
      success = await updateCourse(editingCourse.publicId, data);
    } else {
      success = await createCourse(data);
    }

    if (success) {
      setActiveTab('list');
      setEditingCourse(null);
      fetchCourses(); // Atualiza a lista com o novo item
    }
  };

  return (
    <div className={styles.adminContainer}>
      <Sidebar 
        items={sidebarItems}
        activeItemId={activeTab}
        onItemClick={handleTabChange}
        logo={<h3 style={{ color: '#fff', padding: '0 20px' }}>Admin Panel</h3>}
      />

      <main className={styles.contentArea}>
        <div className={styles.contentWrapper}>
          
          {activeTab === 'list' ? (
            <CourseList 
              courses={courses} // Dados reais do hook
              isLoading={loading}
              onEdit={handleEdit}
              onDelete={handleDelete}
              onNewClick={handleCreateNew}
            />
          ) : (
            <CourseForm 
              initialData={editingCourse}
              isLoading={loading}
              onSubmit={handleFormSubmit}
              onCancel={() => setActiveTab('list')}
            />
          )}

        </div>
      </main>
    </div>
  );
};
