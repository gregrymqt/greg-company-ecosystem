export interface User {
  Id: string; //
  Name: string; //
  Email: string; //
  EmailConfirmed: boolean; //
  CreatedAt?: string; //
}

export interface UserSummary {
  TotalUsers: number; //
  ConfirmedEmails: number; //
  ConfirmationRate: number; //
  NewUsersLast30Days: number; //
}

export interface UserWSPayload {
  summary: UserSummary; //
  type: "Manual" | "Auto"; //
}