export interface Course {
  Id: string; //
  Name: string; //
  IsActive: boolean; //
  TotalVideos: number; //
}

export interface ContentSummary {
  TotalCourses: number; //
  ActiveCourses: number; //
  TotalVideosLib: number; //
  AvgVideosPerCourse: number; //
}

export interface ContentWebSocketPayload {
  summary: ContentSummary; //
  type: "Manual" | "Auto"; //
}