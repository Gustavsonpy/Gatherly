import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DatePipe, TitleCasePipe } from '@angular/common';
import { EventService } from '../../core/event/event.service';
import { EventModel, ParticipantModel } from '../../core/event/event.model';
import { Sidebar } from '../../components/sidebar/sidebar';
import { CommentService } from '../../core/comments/comment.service';
import { CommentModel } from '../../core/comments/comment.model';
import { CommentSender } from '../../components/comment/comment-sender';
import { getErrorMessage } from '../../core/utils/http-error';
import { finalize } from 'rxjs';
import { Icon } from '../../components/icon/icon';
import { GenericButton } from '../../components/buttons/generic-button';

type TabId = 'about' | 'participants' | 'chat';

@Component({
  selector: 'app-event-detail',
  imports: [Sidebar, CommentSender, DatePipe, TitleCasePipe, Icon, GenericButton],
  templateUrl: './event-detail.html',
})
export class EventDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private eventService = inject(EventService);
  private commentService = inject(CommentService)

  readonly tabs: { id: TabId; label: string; icon: string; empty: string }[] = [
    {
      id: 'about',
      label: 'Sobre',
      icon: 'people-md',
      empty: 'Nenhum detalhe para esse evento ainda.',
    },
    {
      id: 'participants',
      label: 'Participantes',
      icon: 'crown-sm',
      empty: 'Nenhum participante ainda.',
    },
    {
      id: 'chat',
      label: 'Chat',
      icon: 'crown-sm',
      empty: 'Nenhum comentário feito ainda.',
    },
  ];
  
  event = signal<EventModel | null>(null);
  participants = signal<ParticipantModel[]>([]);
  participantsCount = signal(0);
  joined = signal(false);
  justJoined = signal(false);
  loadError = signal<string | null>(null);
  participantsError = signal<string | null>(null);
  joinError = signal<string | null>(null);
  joining = signal(false);
  comments = signal<CommentModel[]>([]);
  status = signal('outcoming');
  activeTab = signal<TabId>('about');

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;

    this.eventService.getEventById(id).subscribe({
      next: (data) => {
        this.event.set(data);
        this.participantsCount.set(data.participantsCount);
        this.joined.set(data.isParticipating);
        this.status.set(data.status);
        console.log(`Status: ${JSON.stringify(data)}`);
      },
      error: (err) =>
        this.loadError.set(getErrorMessage(err, 'Não foi possível carregar o evento.')),
    });

    this.loadParticipants(id);
    this.loadComments(id);
  }

  setActiveTab(value: TabId): void {
    this.activeTab.set(value);
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

  private loadComments(eventId: string): void {
    this.commentService.getCommentsByEvent(eventId).subscribe({
      next: (list) => this.comments.set(list),
      error: (err) => console.error(err),
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

  onCommentCreated(comment: CommentModel): void {
    this.comments.update((list) => [...list, comment]);
  }
}