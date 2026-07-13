import { ApiService } from '@/shared/services/api.service';
import type {
  AboutSectionData,
  TeamMember,
  AboutPageResponse,
} from '@/features/about/types/about.types';

const ENDPOINT = "/About";

export const AboutService = {
  // LEITURA
  getAboutPage: async (): Promise<AboutPageResponse> => {
    return await ApiService.get<AboutPageResponse>(`${ENDPOINT}`);
  },
};
