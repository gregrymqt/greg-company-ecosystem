import { ApiService } from "@/shared/services/api.service";
import type {
  CreatePixDTO,
  PixResponse,
  IdentificationType,
} from "../types";

export const PixService = {
  createPix: async (data: CreatePixDTO, idempotencyKey: string): Promise<PixResponse> => {
    // 1. Chave de idempotência agora vem da camada superior (hook/component)
    const headers = {
      "X-Idempotency-Key": idempotencyKey,
    };
    // 2. Enviamos o Header que o Backend exige
    // Nota: Estou assumindo que seu ApiService aceita um terceiro parâmetro de config (como o axios)
    // Se o seu ApiService for customizado, precisaremos ajustar isso lá.
    return await ApiService.post<PixResponse>("/pix/createpix", data, {
      headers,
    });
  },

  getDocTypes: async (): Promise<IdentificationType[]> => {
    return [
      { id: "CPF", name: "CPF" },
      { id: "CNPJ", name: "CNPJ" },
    ];
  },
};
