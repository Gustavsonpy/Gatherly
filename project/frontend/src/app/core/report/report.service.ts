import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CreateEventReport, CreateReport, ReportReasonModel } from './report-reason.model';

@Injectable({providedIn: 'root'})
export class ReportService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5008/api';
  private reportUrl = `${this.apiUrl}/report`;

  getReportReasons() {
    return this.http.get<ReportReasonModel[]>(`${this.apiUrl}/report-reasons`);
  }

  createReport(report: CreateReport) {
    return this.http.post<CreateReport>(`${this.reportUrl}/create`, report);
  }

  createEventReport(report: CreateEventReport) {
    return this.http.post<CreateEventReport>(`${this.reportUrl}/event`, report);
  }
}