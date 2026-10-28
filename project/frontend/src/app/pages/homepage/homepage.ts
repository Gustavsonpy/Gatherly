import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { Router } from '@angular/router';
import { Sidebar } from '../../components/sidebar/sidebar';
import { EventService } from '../../core/event/event.service';
import { EventModel } from '../../core/event/event.model';
import { CategoryService } from '../../core/category/category.service';
import { CategoryModel } from '../../core/category/CategoryModel';
import { RecentlyViewedService } from '../../core/event/recently-viewed.service';

const MONTHS = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
const MONTHS_LONG = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

@Component({
  selector: 'app-home',
  imports: [Sidebar, NgTemplateOutlet],
  templateUrl: './homepage.html',
})
export class Homepage implements OnInit {
  private readonly router = inject(Router);
  private readonly eventService = inject(EventService);
  private readonly categoryService = inject(CategoryService);
  private readonly recentlyViewed = inject(RecentlyViewedService);
  private readonly destroyRef = inject(DestroyRef);

  events = signal<EventModel[]>([]);
  myEvents = signal<EventModel[]>([]);
  categories = signal<CategoryModel[]>([]);
  loading = signal(true);
  failed = signal(false);
  actionError = signal<string | null>(null);

  query = signal('');
  selectedCategory = signal<string | null>(null); // nome da categoria; null = todas
  slide = signal(0);

  categoryTiles = computed(() => [{ id: 'all', name: 'Todos' }, ...this.categories()]);

  private readonly upcoming = computed(() => {
    const now = Date.now();
    return this.events().filter((e) => new Date(e.dateTimeStart).getTime() > now);
  });

  banners = computed(() =>
    [...this.upcoming()].sort((a, b) => b.participantsCount - a.participantsCount).slice(0, 4),
  );
  currentBanner = computed(() => this.banners()[this.slide()] ?? null);

  featured = computed(() => {
    const cat = this.selectedCategory();
    const q = this.query().trim().toLowerCase();
    return [...this.upcoming()]
      .filter((e) => !cat || e.categoryName === cat)
      .filter((e) => !q || `${e.title} ${e.localization}`.toLowerCase().includes(q))
      .sort((a, b) => b.participantsCount - a.participantsCount)
      .slice(0, 8);
  });

  nextEvents = computed(() =>
    this.myEvents()
      .filter((e) => e.status === 'upcoming')
      .sort((a, b) => a.dateTimeStart.localeCompare(b.dateTimeStart))
      .slice(0, 4),
  );

  viewedEvents = computed(() => {
    const byId = new Map(this.events().map((e) => [e.id, e]));
    return this.recentlyViewed.ids().map((id) => byId.get(id)).filter((e): e is EventModel => !!e).slice(0, 4);
  });

  ngOnInit(): void {
    this.eventService.getEvents().subscribe({
      next: (data) => { this.events.set(data); this.loading.set(false); },
      error: (err) => { console.error(err); this.failed.set(true); this.loading.set(false); },
    });
    this.eventService.getMyEvents().subscribe({
      next: (data) => this.myEvents.set(data),
      error: (err) => console.error(err),
    });
    this.categoryService.getCategories().subscribe({
      next: (data) => this.categories.set(data),
      error: (err) => console.error(err),
    });

    const timer = setInterval(() => this.nextSlide(), 6000);
    this.destroyRef.onDestroy(() => clearInterval(timer));
  }

  nextSlide(): void {
    const total = this.banners().length;
    if (total > 1) this.slide.update((i) => (i + 1) % total);
  }

  selectCategory(tile: { id: string; name: string }): void {
    this.selectedCategory.set(tile.id === 'all' ? null : tile.name);
  }

  categoryIcon(name: string): string {
    const slug = name.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
    console.log(`Icon path: icons/${slug}.svg`)
    return `icons/${slug}.svg`;
  }

  day = (e: EventModel) => String(new Date(e.dateTimeStart).getDate()).padStart(2, '0');
  month = (e: EventModel) => MONTHS[new Date(e.dateTimeStart).getMonth()];
  time = (e: EventModel) => {
    const d = new Date(e.dateTimeStart);
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
  };
  longDate = (e: EventModel) => {
    const d = new Date(e.dateTimeStart);
    return `${d.getDate()} de ${MONTHS_LONG[d.getMonth()]}, ${d.getFullYear()}`;
  };

  open(id: string): void {
    this.router.navigate(['/events', id]);
  }

  join(e: EventModel): void {
    if (e.isParticipating) return this.open(e.id);
    this.actionError.set(null);
    this.eventService.joinEvent(e.id).subscribe({
      next: () =>
        this.events.update((list) =>
          list.map((x) => (x.id === e.id ? { ...x, isParticipating: true, participantsCount: x.participantsCount + 1 } : x)),
        ),
      error: (err) => this.actionError.set(err.error?.errors?.[0] ?? 'Não foi possível entrar no evento.'),
    });
  }
}