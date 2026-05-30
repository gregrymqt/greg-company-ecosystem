export interface Ticket {
  Id: string; //
  UserId: string; //
  Context: string; //
  Explanation: string; //
  Status: string; //
  CreatedAt: string; //
}

export interface TicketSummary {
  TotalTickets: number; //
  OpenTickets: number; //
  InProgressTickets: number; //
  ClosedTickets: number; //
  ResolutionRate: number; //
}

export interface SupportWSPayload {
  summary: TicketSummary; //
  type: "Manual" | "Auto"; //
}