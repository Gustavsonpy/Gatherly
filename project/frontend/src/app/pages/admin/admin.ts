import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { Sidebar } from '../../components/sidebar/sidebar';
import { AdminService } from '../../core/admin/admin.service';
import {
  AdminComment,
  AdminEvent,
  AdminEventStatus,
  AdminReport,
  AdminStats,
  AdminUser,
  ReportStatus,
} from '../../core/admin/admin.model';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/toaster/toaster.service';

export type AdminTab = 'reports' | 'users' | 'events' | 'comments';

@Component({
  selector: 'app-admin',
  imports: [Sidebar, DatePipe, RouterLink],
  templateUrl: './admin.html',
})
export class Admin implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);

  readonly tabs: { id: AdminTab; label: string }[] = [
    { id: 'reports', label: 'Denúncias' },
    { id: 'users', label: 'Usuários' },
    { id: 'events', label: 'Eventos' },
    { id: 'comments', label: 'Comentários' },
  ];

  readonly reportStatusLabels: Record<ReportStatus, string> = {
    Pending: 'Pendente',
    Resolved: 'Resolvida',
    Dismissed: 'Descartada',
  };

  readonly eventStatusLabels: Record<AdminEventStatus, string> = {
    upcoming: 'Próximo',
    ongoing: 'Em andamento',
    finished: 'Encerrado',
    canceled: 'Cancelado',
  };

  private readonly searchPlaceholders: Record<AdminTab, string> = {
    reports: '',
    users: 'Buscar por nome ou e-mail',
    events: 'Buscar por título ou cidade',
    comments: 'Buscar por texto ou autor',
  };

  activeTab = signal<AdminTab>('reports');
  stats = signal<AdminStats | null>(null);
  reports = signal<AdminReport[]>([]);
  users = signal<AdminUser[]>([]);
  events = signal<AdminEvent[]>([]);
  comments = signal<AdminComment[]>([]);
  reportStatus = signal<ReportStatus | ''>('Pending');
  search = signal('');
  loading = signal(true);
  failed = signal(false);
  /** Id do item com uma ação em andamento, para desabilitar os botões dele. */
  busyId = signal<string | null>(null);

  readonly searchPlaceholder = computed(() => this.searchPlaceholders[this.activeTab()]);

  private readonly currentUserId = computed(() => this.authService.currentUser()?.id?.toLowerCase());

  ngOnInit(): void {
    this.loadStats();
    this.load();
  }

  setActiveTab(tab: AdminTab): void {
    if (tab === this.activeTab()) return;
    this.activeTab.set(tab);
    this.search.set('');
    this.load();
  }

  onSearch(value: string): void {
    this.search.set(value);
    this.load();
  }

  onReportStatusChange(value: string): void {
    this.reportStatus.set(value as ReportStatus | '');
    this.load();
  }

  isSelf(userId: string): boolean {
    return userId.toLowerCase() === this.currentUserId();
  }

  canCancel(event: AdminEvent): boolean {
    return event.status === 'upcoming' || event.status === 'ongoing';
  }

  load(): void {
    this.loading.set(true);
    this.failed.set(false);

    const done = <T>(target: { set(value: T): void }) => ({
      next: (data: T) => {
        target.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.failed.set(true);
        this.loading.set(false);
      },
    });

    switch (this.activeTab()) {
      case 'reports':
        this.adminService.getReports(this.reportStatus() || null).subscribe(done(this.reports));
        break;
      case 'users':
        this.adminService.getUsers(this.search()).subscribe(done(this.users));
        break;
      case 'events':
        this.adminService.getEvents(this.search()).subscribe(done(this.events));
        break;
      case 'comments':
        this.adminService.getComments(this.search()).subscribe(done(this.comments));
        break;
    }
  }

  private loadStats(): void {
    this.adminService.getStats().subscribe({
      next: (stats) => this.stats.set(stats),
      error: () => this.stats.set(null),
    });
  }

  // ---------- Ações ----------

  updateReport(report: AdminReport, status: ReportStatus): void {
    const message = status === 'Resolved' ? 'Denúncia marcada como resolvida.'
      : status === 'Dismissed' ? 'Denúncia descartada.'
      : 'Denúncia reaberta.';
    this.run(report.id, this.adminService.updateReportStatus(report.id, status), message);
  }

  setSuspended(userId: string, userName: string, suspended: boolean): void {
    const question = suspended
      ? `Suspender ${userName}? A pessoa perde o acesso imediatamente.`
      : `Reativar ${userName}?`;
    if (!confirm(question)) return;

    this.run(
      userId,
      this.adminService.setSuspended(userId, suspended),
      suspended ? `${userName} foi suspenso(a).` : `${userName} foi reativado(a).`,
    );
  }

  setAdmin(user: AdminUser, isAdmin: boolean): void {
    const question = isAdmin
      ? `Tornar ${user.name} administrador(a)?`
      : `Remover o acesso de administrador de ${user.name}?`;
    if (!confirm(question)) return;

    this.run(
      user.id,
      this.adminService.setAdmin(user.id, isAdmin),
      isAdmin ? `${user.name} agora é administrador(a).` : `${user.name} não é mais administrador(a).`,
    );
  }

  cancelEvent(eventId: string, title: string, busyKey = eventId): void {
    if (!confirm(`Cancelar o evento "${title}"? Ninguém mais poderá participar.`)) return;
    this.run(busyKey, this.adminService.cancelEvent(eventId), `Evento "${title}" cancelado.`);
  }

  deleteComment(comment: AdminComment): void {
    if (!confirm(`Excluir o comentário de ${comment.userName}? Essa ação não pode ser desfeita.`)) return;
    this.run(comment.id, this.adminService.deleteComment(comment.id), 'Comentário excluído.');
  }

  private run(id: string, action: Observable<void>, successMessage: string): void {
    this.busyId.set(id);

    action.subscribe({
      next: () => {
        this.busyId.set(null);
        this.toast.success(successMessage);
        this.loadStats();
        this.load();
      },
      error: (err) => {
        this.busyId.set(null);
        this.toast.error(err?.error?.errors?.[0] ?? 'Não foi possível concluir a ação.');
      },
    });
  }
}
