import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { EventService } from '../../core/event/event.service';
import { EventModel } from '../../core/event/event.model';
import { Sidebar } from '../../components/sidebar/sidebar';
import { GenericButton } from '../../components/buttons/generic-button';

@Component({
  selector: 'app-event-detail',
  imports: [DatePipe, Sidebar, GenericButton],
  templateUrl: './event-detail.html',
})
export class EventDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private eventService = inject(EventService);

  event = signal<EventModel | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.eventService.getEventById(id).subscribe({
      next: (data) => this.event.set(data),
      error: (err) => console.error(err),
    });
  }
}