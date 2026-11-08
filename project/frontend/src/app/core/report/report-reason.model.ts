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
  customReason: string | null;
  description: string;
}