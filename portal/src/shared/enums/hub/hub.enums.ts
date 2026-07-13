export const AppHubsCSharp = {
  GlobalRealtime: "/ws/realtime",
} as const;

// Tipo unificado para o Service
export type AnyAppHub =
  | (typeof AppHubsCSharp)[keyof typeof AppHubsCSharp]
