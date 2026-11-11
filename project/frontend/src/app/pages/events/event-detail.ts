import { Component, computed, inject, OnInit, signal } from '@angular/core';
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
import { AuthService } from '../../core/auth/auth.service';
import { ReportModal } from '../../components/report-modal/report-modal';

type TabId = 'about' | 'participants' | 'chat';

@Component({
  selector: 'app-event-detail',
  imports: [Sidebar, CommentSender, DatePipe, TitleCasePipe, Icon, GenericButton, ReportModal],
  templateUrl: './event-detail.html',
})
export class EventDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private eventService = inject(EventService);
  private commentService = inject(CommentService);
  private authService = inject(AuthService);

  readonly tabs: { id: TabId; label: string; icon: string; empty: string }[] = [
    {
      id: 'about',
      label: 'Sobre',
      icon: 'info-sm',
      empty: 'Nenhum detalhe para esse evento ainda.',
    },
    {
      id: 'participants',
      label: 'Participantes',
      icon: 'people-sm',
      empty: 'Nenhum participante ainda.',
    },
    {
      id: 'chat',
      label: 'Chat',
      icon: 'chat-sm',
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
  reportedUser = signal<ParticipantModel | null>(null);

  activeReportModal = signal<boolean>(false);

  private readonly userId = computed(() => this.authService.currentUser()?.id);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;

    this.eventService.getEventById(id).subscribe({
      next: (data) => {
        this.event.set(data);
        this.participantsCount.set(data.participantsCount);
        this.joined.set(data.isParticipating);
        this.status.set(data.status);
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

  leave(): void {
    if (!this.joined()) return;

    const id = this.event()!.id;

    this.eventService.leaveEvent(id).subscribe({
      next: () => {
        this.participantsCount.update((c) => c - 1);
        this.loadParticipants(id);
        this.joined.set(false);
      },
      error: (err) => this.participantsError.set(
          getErrorMessage(err, 'Não foi possível sair do evento.'),
        ),
    });
  }

  onCommentCreated(comment: CommentModel): void {
    this.comments.update((list) => [...list, comment]);
  }

  isOrganizer(): boolean {
    return this.event()?.userId.toLowerCase() === this.userId()?.toLowerCase();
  }
  
  openReportModal(user: ParticipantModel): void {
    this.reportedUser.set(user);
    this.activeReportModal.set(true);
  }

  closeReportModal(): void {
    this.activeReportModal.set(false);
    this.reportedUser.set(null);
  }
}