import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { EventService } from '../../core/event/event.service';
import { EventModel, ParticipantModel } from '../../core/event/event.model';
import { Sidebar } from '../../components/sidebar/sidebar';
import { CommentService } from '../../core/comments/comment.service';
import { CommentModel } from '../../core/comments/comment.model';
import { CommentSender } from '../../components/comment/comment-sender';

@Component({
  selector: 'app-event-detail',
  imports: [Sidebar, CommentSender, DatePipe],
  templateUrl: './event-detail.html',
})
export class EventDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private eventService = inject(EventService);
  private commentService = inject(CommentService)

  event = signal<EventModel | null>(null);
  participants = signal<ParticipantModel[]>([]);
  participantsCount = signal(0);
  joined = signal(false);
  justJoined = signal(false);
  comments = signal<CommentModel[]>([]);

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
    this.loadComments(id);
  }

  private loadParticipants(id: string): void {
    this.eventService.getParticipants(id).subscribe({
      next: (list) => this.participants.set(list),
      error: (err) => console.error(err),
    });
  }

  private loadComments(eventId: string): void {
    this.commentService.getCommentsByEvent(eventId).subscribe({
      next: (list) => this.comments.set(list),
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

  onCommentCreated(comment: CommentModel): void {
    this.comments.update((list) => [...list, comment]);
  }
}