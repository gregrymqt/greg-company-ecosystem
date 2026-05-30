/**
 * Admin Students Service
 * Alinhado com AdminStudentsController.cs
 * Endpoints: /admin/students/*
 */

import { ApiService } from '@/shared/services/api.service';
import type { StudentDto, PaginatedResult, StudentFilters } from '../../shared';

class AdminStudentsService {
  private readonly BASE_PATH = '/admin/students';

  /**
   * Lista todos os estudantes com paginação e filtros
   * GET /admin/students
   */
  async getAll(filters: StudentFilters): Promise<PaginatedResult<StudentDto>> {
    const params = new URLSearchParams({
      pageNumber: filters.pageNumber.toString(),
      pageSize: filters.pageSize.toString(),
    });

    if (filters.searchTerm) {
      params.append('searchTerm', filters.searchTerm);
    }

    if (filters.subscriptionStatus) {
      params.append('subscriptionStatus', filters.subscriptionStatus);
    }

    return await ApiService.get<PaginatedResult<StudentDto>>(
      `${this.BASE_PATH}?${params.toString()}`
    );
  }

  /**
   * Busca estudante por ID
   * GET /admin/students/{id}
   */
  async getById(id: string): Promise<StudentDto> {
    return await ApiService.get<StudentDto>(`${this.BASE_PATH}/${id}`);
  }
}

export const adminStudentsService = new AdminStudentsService();
