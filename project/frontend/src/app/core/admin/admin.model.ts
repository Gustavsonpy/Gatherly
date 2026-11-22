export type ReportStatus = 'Pending' | 'Resolved' | 'Dismissed';
export type AdminEventStatus = 'upcoming' | 'ongoing' | 'finished' | 'canceled';

export interface AdminStats {
  pendingReports: number;
  totalUsers: number;
  suspendedUsers: number;
  upcomingEvents: number;
  totalEvents: number;
}

export interface AdminReport {
  id: string;
  reporterId: string;
  reporterName: string;
  reportedUserId: string | null;
  reportedUserName: string | null;
  reportedUserSuspended: boolean;
  eventId: string;
  eventTitle: string;
  eventCanceled: boolean;
  reason: number;
  reasonName: string;
  description: string;
  status: ReportStatus;
  createdAt: string;
}

export interface AdminUser {
  id: string;
  name: string;
  email: string;
  city: string | null;
  urlPhoto: string | null;
  isAdmin: boolean;
  isSuspended: boolean;
  registerDate: string;
  eventsCreated: number;
  reportsReceived: number;
}

export interface AdminEvent {
  id: string;
  title: string;
  city: string;
  dateTimeStart: string;
  dateTimeEnd: string;
  canceledAt: string | null;
  organizerId: string;
  organizerName: string;
  categoryName: string;
  participantsCount: number;
  maxCapacity: number;
  reportsCount: number;
  status: AdminEventStatus;
}

export interface AdminComment {
  id: string;
  text: string;
  registerDate: string;
  userId: string;
  userName: string;
  eventId: string;
  eventTitle: string;
}
