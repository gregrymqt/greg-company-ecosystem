export const AppHubsCSharp = {
  Payment: "/PaymentProcessingHub",
  Refund: "/RefundProcessingHub",
  Video: "/videoProcessingHub",
} as const;

export const AppHubsBIFastAPI = {
  BI_CLAIMS: "/hubs/claims", // Ajustado para bater com seu setup_websocket_routes
  BI_FINANCIAL: "/hubs/financial",
  BI_SUPPORT: "/hubs/support",
  BI_SUBSCRIPTIONS: "/hubs/subscriptions",
  BI_USERS: "/hubs/users",
  BI_CONTENT: "/hubs/content",
  STORAGE: "/hubs/storage",
} as const;

// Tipo unificado para o Service
export type AnyAppHub =
  | (typeof AppHubsCSharp)[keyof typeof AppHubsCSharp]
  | (typeof AppHubsBIFastAPI)[keyof typeof AppHubsBIFastAPI];
