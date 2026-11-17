import { Component, inject, input, OnInit, output, signal } from '@angular/core';
import { GenericButton } from '../buttons/generic-button';
import { ParticipantModel } from '../../core/event/event.model';
import { Icon } from '../icon/icon';
import { ReportService } from '../../core/report/report.service';
import { CreateReport, ReportReasonModel } from '../../core/report/report-reason.model';
import { NgClass } from '@angular/common';
import { ToastService } from '../../core/toaster/toaster.service';
import { ToasterErrorEventMessage, ToasterSuccessEventMessage } from '../../enum/toaster/toaster-enum';

@Component({
  selector: 'report-modal',
  standalone: true,
  imports: [GenericButton, Icon, NgClass],
  templateUrl: './report-modal.html',
})
export class ReportModal implements OnInit {
  private reportService = inject(ReportService);
  private toastService = inject(ToastService);

  reportedUser = input.required<ParticipantModel>();
  eventId = input.required<string>();

  close = output<void>();
  reported = output<void>();

  reportReasons = signal<ReportReasonModel[]>([]);
  selectedReason = signal<ReportReasonModel | null>(null);
  description = signal('');
  loading = signal(false);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.reportService.getReportReasons().subscribe({
      next: (value) => this.reportReasons.set(value),
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Não foi possível carregar os motivos da denúncia.');
      },
    });
  }

  activateReason(reason: ReportReasonModel): void {
    this.selectedReason.set(reason);
  }

  onDescriptionInput(event: Event): void {
    this.description.set((event.target as HTMLTextAreaElement).value);
  }

  submitReport(): void {
    const reason = this.selectedReason();
    if (!reason || this.loading()) return;

    const report: CreateReport = {
      reportedUserId: this.reportedUser().id,
      eventId: this.eventId(),
      reason: reason.id,
      customReason: null,
      description: this.description().trim(),
    };

    this.loading.set(true);
    this.errorMessage.set(null);

    this.reportService.createReport(report).subscribe({
      next: () => {
        this.loading.set(false);
        this.reported.emit();
        this.close.emit();
        this.toastService.success(ToasterSuccessEventMessage.report_user);
      },
      error: (err) => {
        console.error(err);
        this.loading.set(false);
        this.errorMessage.set(
          err.error?.errors?.[0] ?? 'Não foi possível enviar a denúncia. Tente novamente.'
        );
        this.toastService.error(ToasterErrorEventMessage.rating_user);
      },
    });
  }
}