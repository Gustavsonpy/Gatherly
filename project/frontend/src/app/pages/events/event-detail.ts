import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { EventService } from '../../core/event/event.service';
import { EventModel, ParticipantModel } from '../../core/event/event.model';
import { Sidebar } from '../../components/sidebar/sidebar';
import { finalize } from 'rxjs';
import { getErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-event-detail',
  imports: [Sidebar],
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
  loadError = signal<string | null>(null);
  participantsError = signal<string | null>(null);
  joinError = signal<string | null>(null);
  joining = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;

    this.eventService.getEventById(id).subscribe({
      next: (data) => {
        this.event.set(data);
        this.participantsCount.set(data.participantsCount);
        this.joined.set(data.isParticipating);
      },
      error: (err) =>
        this.loadError.set(getErrorMessage(err, 'Não foi possível carregar o evento.')),
    });

    this.loadParticipants(id);
  }

  private loadParticipants(id: string): void {
    this.participantsError.set(null);

    this.eventService.getParticipants(id).subscribe({
      next: (list) => this.participants.set(list),
      error: (err) =>
        this.participantsError.set(
          getErrorMessage(err, 'Não foi possível carregar os participantes.'),
        ),
    });
  }

  join(): void {
    if (this.joining()) return;

    const id = this.event()!.id;
    this.joinError.set(null);
    this.joining.set(true);

    this.eventService
      .joinEvent(id)
      .pipe(finalize(() => this.joining.set(false)))
      .subscribe({
        next: () => {
          this.joined.set(true);
          this.justJoined.set(true);
          this.participantsCount.update((c) => c + 1);
          this.loadParticipants(id);
          setTimeout(() => this.justJoined.set(false), 4000);
        },
        error: (err) =>
          this.joinError.set(getErrorMessage(err, 'Não foi possível participar do evento.')),
      });
  }
}