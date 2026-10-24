import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { EventModel, ParticipantModel } from '../../core/event/event.model';
import { EventService } from '../../core/event/event.service';
import { DatePipe, TitleCasePipe } from '@angular/common';
import { GenericButton } from '../buttons/generic-button';

const MAX_AVATARS = 4;

@Component({
  selector: 'event-row',
  imports: [DatePipe, TitleCasePipe, GenericButton],
  templateUrl: './event-row.html',
})
export class EventRow implements OnInit {
  private readonly router = inject(Router);
  private readonly eventService = inject(EventService);

  event = input.required<EventModel>();
  isOrganizer = input(false);

  participants = signal<ParticipantModel[]>([]);

  visibleParticipants = computed(() => this.participants().slice(0, MAX_AVATARS));
  extraParticipants = computed(() => Math.max(0, this.participants().length - MAX_AVATARS));

  isFull = computed(() => this.event().participantsCount >= this.event().maxCapacity);

  ngOnInit(): void {
    this.eventService.getParticipants(this.event().id).subscribe({
      next: (data) => this.participants.set(data),
      error: (err) => console.error(err),
    });
  }

  goToEvent(): void {
    this.router.navigate(['/events', this.event().id]);
  }
}