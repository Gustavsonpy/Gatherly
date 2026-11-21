export interface ReportReasonModel {
  id: number;
  name: string;
  description: string;
  icon: string;
}

export interface CreateReport {
  reportedUserId: string;
  eventId: string;
  reason: number;
  description: string;
}

export interface CreateEventReport {
  eventId: string;
  reason: number;
  description: string;
}