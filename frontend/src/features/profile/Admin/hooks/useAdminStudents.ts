/**
 * Hook para gerenciar lista de estudantes (Admin)
 * Usa adminStudentsService (AdminStudentsController)
 */

import { useState, useEffect, useCallback } from 'react';
import { adminStudentsService } from '../services/admin-students.service';
import type { StudentDto, PaginatedResult, StudentFilters } from '../../shared';
import { AlertService } from '@/shared/services/alert.service';

const DEFAULT_FILTERS: StudentFilters = {
  pageNumber: 1,
  pageSize: 10,
  searchTerm: '',
  subscriptionStatus: ''
};

export const useAdminStudents = (initialFilters?: Partial<StudentFilters>) => {
  const [students, setStudents] = useState<PaginatedResult<StudentDto> | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<StudentFilters>({
    ...DEFAULT_FILTERS,
    ...initialFilters
  });

  const fetchStudents = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await adminStudentsService.getAll(filters);
      setStudents(data);
    } catch (err) {
      const errorMessage = (err as Error).message || 'Erro ao buscar estudantes';
      setError(errorMessage);
      AlertService.error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    fetchStudents();
  }, [fetchStudents]);

  const updateFilters = useCallback((newFilters: Partial<StudentFilters>) => {
    setFilters(prev => ({ ...prev, ...newFilters }));
  }, []);

  const nextPage = useCallback(() => {
    if (students && filters.pageNumber < students.totalPages) {
      updateFilters({ pageNumber: filters.pageNumber + 1 });
    }
  }, [students, filters.pageNumber, updateFilters]);

  const previousPage = useCallback(() => {
    if (filters.pageNumber > 1) {
      updateFilters({ pageNumber: filters.pageNumber - 1 });
    }
  }, [filters.pageNumber, updateFilters]);

  const goToPage = useCallback((page: number) => {
    updateFilters({ pageNumber: page });
  }, [updateFilters]);

  return {
    students,
    loading,
    error,
    filters,
    updateFilters,
    nextPage,
    previousPage,
    goToPage,
    refetch: fetchStudents
  };
};
