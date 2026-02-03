import type { UserSummary, User } from "../types/users.types";
import { BiApiService } from "@/shared/services/api.service";

export const usersService = {
  // Busca KPIs consolidados de usuários
  getSummary: async (): Promise<UserSummary> => {
    const response = await BiApiService.get<UserSummary>("/users/summary");
    return response;
  },

  // Busca lista de usuários cadastrados
  getUsersList: async (): Promise<User[]> => {
    const response = await BiApiService.get<User[]>("/users/list");
    return response;
  },

  // Sincronização de base de usuários para o Rows.com
  syncToRows: async () => {
    const response = await BiApiService.post<{ message: string }>(
      "/users/sync-rows",
      {},
    );
    return response;
  },
};
