import { ApiService } from "@/shared/services/api.service";
import type { SubscriptionDetailsDto, UpdateSubscriptionStatusDto, SubscriptionStatusType } from "../types/subscriptions.types";

export const userSubscriptionService = {

  getDetails: async (): Promise<SubscriptionDetailsDto> => {
    // Controller: [Route("api/subscriptions")] + [HttpGet("details")]
    // URL Final deve ser: /api/subscriptions/details
    return await ApiService.get<SubscriptionDetailsDto>('/subscriptions/details'); 
  },

  updateStatus: async (status: SubscriptionStatusType): Promise<void> => {
    const body: UpdateSubscriptionStatusDto = {
      status: status || ''
    };
    // Controller: [Route("api/subscriptions")] + [HttpPut("status")]
    // URL Final deve ser: /api/subscriptions/status
    return await ApiService.put<void>('/subscriptions/status', body);
  }
};
