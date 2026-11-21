import {
  Component,
  DestroyRef,
  ElementRef,
  afterRenderEffect,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { CardEvent } from '../card-event/card-event';
import { EventModel } from '../../core/event/event.model';

@Component({
  selector: 'event-carousel',
  standalone: true,
  imports: [CardEvent],
  templateUrl: './event-carousel.html',
  styles: `
    /* Esconde a barra de rolagem sem perder o scroll (touch, trackpad, teclado). */
    .no-scrollbar { scrollbar-width: none; }
    .no-scrollbar::-webkit-scrollbar { display: none; }
  `,
})
export class EventCarousel {
  title = input.required<string>();
  events = input.required<EventModel[]>();
  emptyMessage = input('Nenhum evento por aqui ainda.');

  private readonly track = viewChild<ElementRef<HTMLElement>>('track');

  readonly canPrev = signal(false);
  readonly canNext = signal(false);

  constructor() {
    const destroyRef = inject(DestroyRef);
    const observer = new ResizeObserver(() => this.updateArrows());
    destroyRef.onDestroy(() => observer.disconnect());

    afterRenderEffect(() => {
      this.events();
      const el = this.track()?.nativeElement;
      if (!el) return;
      observer.disconnect();
      observer.observe(el);
      this.updateArrows();
    });
  }

  updateArrows(): void {
    const el = this.track()?.nativeElement;
    if (!el) return;
    this.canPrev.set(el.scrollLeft > 1);
    this.canNext.set(el.scrollLeft + el.clientWidth < el.scrollWidth - 1);
  }

  /** Avança ou volta quase uma "página" de cards. */
  scroll(direction: -1 | 1): void {
    const el = this.track()?.nativeElement;
    if (!el) return;
    el.scrollBy({ left: direction * el.clientWidth * 0.9, behavior: 'smooth' });
  }
}
