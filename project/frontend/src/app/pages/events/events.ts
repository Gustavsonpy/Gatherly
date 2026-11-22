import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { Sidebar } from '../../components/sidebar/sidebar';
import { CardEvent } from '../../components/card-event/card-event';
import { EventCarousel } from '../../components/event-carousel/event-carousel';
import { EventService } from '../../core/event/event.service';
import { EventModel } from '../../core/event/event.model';
import { CategoryService } from '../../core/category/category.service';
import { CategoryModel } from '../../core/category/CategoryModel';
import {
  EMPTY_EVENT_FILTERS,
  EventFilters,
  cityLabel,
  filterEvents,
  hasActiveFilters,
} from '../../core/event/event-filter';

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [Sidebar, CardEvent, NgTemplateOutlet, EventCarousel],
  templateUrl: 'events.html',
})
export class Events implements OnInit {
  private readonly eventService = inject(EventService);
  private readonly categoryService = inject(CategoryService);

  events = signal<EventModel[]>([]);
  cityEvents = signal<EventModel[]>([]);
  categories = signal<CategoryModel[]>([]);
  loading = signal(true);

  filters = signal<EventFilters>({ ...EMPTY_EVENT_FILTERS });

  readonly filtering = computed(() => hasActiveFilters(this.filters()));

  /** Cidades que têm eventos futuros, para o select. */
  readonly cities = computed(() =>
    [...new Set(this.events().map(cityLabel).filter(Boolean))].sort((a, b) =>
      a.localeCompare(b, 'pt-BR'),
    ),
  );

  readonly results = computed(() =>
    filterEvents(this.events(), this.filters()).sort((a, b) =>
      a.dateTimeStart.localeCompare(b.dateTimeStart),
    ),
  );

  ngOnInit(): void {
    this.eventService.getFutureEvents().subscribe({
      next: (data) => {
        this.events.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        console.error(error);
        this.loading.set(false);
      },
    });

    this.eventService.getEventsByCity().subscribe({
      next: (data) => this.cityEvents.set(data),
      error: (error) => console.error(error),
    });

    this.categoryService.getCategories().subscribe({
      next: (data) => this.categories.set(data),
      error: (error) => console.error(error),
    });
  }

  setFilter<K extends keyof EventFilters>(key: K, value: EventFilters[K]): void {
    this.filters.update((f) => ({ ...f, [key]: value }));
  }

  /** Valor de <select>/<input type="date">: string vazia vira null. */
  setOptional(key: 'categoryId' | 'city' | 'dateFrom' | 'dateTo', event: Event): void {
    const value = (event.target as HTMLInputElement | HTMLSelectElement).value;
    this.setFilter(key, value === '' ? null : value);
  }

  setMinSpots(event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);
    this.setFilter('minSpots', Number.isFinite(value) && value > 0 ? Math.floor(value) : 0);
  }

  clearFilters(): void {
    this.filters.set({ ...EMPTY_EVENT_FILTERS });
  }
}
