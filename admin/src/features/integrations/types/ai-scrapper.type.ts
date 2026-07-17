export interface AICredentialsPayload {
  tenant_id: string;
  provider: string;
  access_token: string;
}

export interface WebScraperPayload {
  tenant_id: string;
  url: string;
}

export interface WebScraperResponse {
  status: string;
  task_id: string;
  message: string;
}