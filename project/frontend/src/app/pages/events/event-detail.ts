import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { EventService } from '../../core/event/event.service';
import { EventModel, ParticipantModel } from '../../core/event/event.model';
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
  participants = signal<ParticipantModel[]>([]);
  participantsCount = signal(0);
  joined = signal(false);
  justJoined = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;

    this.eventService.getEventById(id).subscribe({
      next: (data) => {
        this.event.set(data);
        this.participantsCount.set(data.participantsCount);
        this.joined.set(data.isParticipating);
      },
      error: (err) => console.error(err),
    });

    this.loadParticipants(id);
  }

  private loadParticipants(id: string): void {
    this.eventService.getParticipants(id).subscribe({
      next: (list) => this.participants.set(list),
      error: (err) => console.error(err),
    });
  }

  join(): void {
    const id = this.event()!.id;

    this.eventService.joinEvent(id).subscribe({
      next: () => {
        this.joined.set(true);
        this.justJoined.set(true);
        this.participantsCount.update((c) => c + 1);
        this.loadParticipants(id);
        setTimeout(() => this.justJoined.set(false), 4000);
      },
      error: (err) => console.error(err),
    });
  }
}