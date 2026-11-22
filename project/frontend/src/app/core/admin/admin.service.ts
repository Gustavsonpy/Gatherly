import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdminComment,
  AdminEvent,
  AdminReport,
  AdminStats,
  AdminUser,
  ReportStatus,
} from './admin.model';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/admin`;

  private params(key: string, value?: string | null): HttpParams {
    const trimmed = value?.trim();
    return trimmed ? new HttpParams().set(key, trimmed) : new HttpParams();
  }

  getStats(): Observable<AdminStats> {
    return this.http.get<AdminStats>(`${this.apiUrl}/stats`);
  }

  getReports(status?: ReportStatus | null): Observable<AdminReport[]> {
    return this.http.get<AdminReport[]>(`${this.apiUrl}/reports`, { params: this.params('status', status) });
  }

  updateReportStatus(id: string, status: ReportStatus): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/reports/${id}/status`, { status });
  }

  getUsers(search?: string): Observable<AdminUser[]> {
    return this.http.get<AdminUser[]>(`${this.apiUrl}/users`, { params: this.params('search', search) });
  }

  setSuspended(userId: string, value: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/users/${userId}/suspended`, { value });
  }

  setAdmin(userId: string, value: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/users/${userId}/admin`, { value });
  }

  getEvents(search?: string): Observable<AdminEvent[]> {
    return this.http.get<AdminEvent[]>(`${this.apiUrl}/events`, { params: this.params('search', search) });
  }

  cancelEvent(eventId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/events/${eventId}/cancel`, {});
  }

  getComments(search?: string): Observable<AdminComment[]> {
    return this.http.get<AdminComment[]>(`${this.apiUrl}/comments`, { params: this.params('search', search) });
  }

  deleteComment(commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/comments/${commentId}`);
  }
}
