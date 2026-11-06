import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Sidebar } from '../../components/sidebar/sidebar';
import { EventRow } from '../../components/event-row/event-row';
import { Icon } from '../../components/icon/icon';
import { AuthService } from '../../core/auth/auth.service';
import { EventService } from '../../core/event/event.service';
import { EventModel } from '../../core/event/event.model';

type TabId = 'participated' | 'organized';

interface MonthGroup {
  key: string;
  date: string;
  events: EventModel[];
}

@Component({
  selector: 'app-history',
  imports: [Sidebar, EventRow, Icon, DatePipe],
  templateUrl: './history.html',
})
export class HistoryPage implements OnInit {
  private readonly eventService = inject(EventService);
  private readonly authService = inject(AuthService);

  readonly tabs: { id: TabId; label: string; icon: string; empty: string }[] = [
    {
      id: 'participated',
      label: 'Participou',
      icon: 'people-md',
      empty: 'Você ainda não participou de nenhum evento concluído.',
    },
    {
      id: 'organized',
      label: 'Organizou',
      icon: 'crown-sm',
      empty: 'Você ainda não organizou nenhum evento concluído.',
    },
  ];

  activeTab = signal<TabId>('participated');
  events = signal<EventModel[]>([]);
  loading = signal(true);
  failed = signal(false);

  private readonly userId = computed(() => this.authService.currentUser()?.id);

  activeTabInfo = computed(() => this.tabs.find((t) => t.id === this.activeTab())!);

  visibleEvents = computed(() => {
    const done = this.events().filter((e) => e.status === 'done');

    return this.activeTab() === 'organized'
      ? done.filter((e) => this.isOrganizer(e))
      : done.filter((e) => e.isParticipating);
  });

  groups = computed<MonthGroup[]>(() => {
    const sorted = [...this.visibleEvents()].sort(
      (a, b) => new Date(b.dateTimeStart).getTime() - new Date(a.dateTimeStart).getTime(),
    );

    const byMonth = new Map<string, MonthGroup>();

    for (const event of sorted) {
      const d = new Date(event.dateTimeStart);
      const key = `${d.getFullYear()}-${d.getMonth()}`;
      let group = byMonth.get(key);

      if (!group) {
        group = { key, date: event.dateTimeStart, events: [] };
        byMonth.set(key, group);
      }

      group.events.push(event);
    }

    return [...byMonth.values()];
  });

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
  }

  isOrganizer(e: EventModel): boolean {
    return e.userId.toLowerCase() === this.userId()?.toLowerCase();
  }
}