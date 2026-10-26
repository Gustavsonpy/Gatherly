import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Sidebar } from '../../components/sidebar/sidebar';
import { AuthService } from '../../core/auth/auth.service';
import { EventService } from '../../core/event/event.service';
import { EventModel } from '../../core/event/event.model';
import { EventRow } from '../../components/event-row/event-row';

type TabId = 'upcoming' | 'ongoing' | 'canceled';

@Component({
  selector: 'app-my-events',
  imports: [Sidebar, EventRow],
  templateUrl: './my-events.html',
})
export class MyEvents implements OnInit {
  private readonly eventService = inject(EventService);
  private readonly authService = inject(AuthService);

  readonly tabs: { id: TabId; label: string; icon: string; color: string }[] = [
    { id: 'upcoming', label: 'Próximos', icon: 'icons/calendar-sm.svg', color: 'text-primary' },
    { id: 'ongoing', label: 'Em andamento', icon: 'icons/in_progress-sm.svg', color: 'text-green-600' },
    { id: 'canceled', label: 'Cancelado', icon: 'icons/canceled-sm.svg', color: 'text-red-500' },
  ];

  activeTab = signal<TabId>('upcoming');
  events = signal<EventModel[]>([]);
  loading = signal(true);
  failed = signal(false);
  actionError = signal<string | null>(null);

  private readonly userId = computed(() => this.authService.currentUser()?.id);

  // O id de cada aba é igual ao status que vem do backend,
  // então basta filtrar por ele.
  visibleEvents = computed(() =>
    this.events().filter((e) => e.status === this.activeTab()),
  );

  ngOnInit(): void {
    this.eventService.getMyEvents().subscribe({
      next: (data) => {
        this.events.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.failed.set(true);
        this.loading.set(false);
      },
    });
  }

  setActiveTab(value: TabId): void {
    this.activeTab.set(value);
    this.actionError.set(null);
  }

  cancelEvent(event: EventModel): void {
    if (!confirm(`Cancelar "${event.title}"? Ninguém mais poderá entrar.`)) return;

    this.actionError.set(null);

    this.eventService.cancelEvent(event.id).subscribe({
      next: () =>
        this.events.update((list) =>
          list.map((e) => (e.id === event.id ? { ...e, status: 'canceled' } : e)),
        ),
      error: (err) =>
        this.actionError.set(err.error?.errors?.[0] ?? 'Não foi possível cancelar o evento.'),
    });
  }

  isOrganizer(e: EventModel): boolean {
    return e.userId.toLowerCase() === this.userId()?.toLowerCase();
  }
}