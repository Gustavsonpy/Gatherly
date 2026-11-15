import { Component, computed, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { CommentModel } from '../../core/comments/comment.model';
import { CommentSender } from '../comment/comment-sender';

@Component({
  selector: 'event-comments',
  imports: [DatePipe, CommentSender],
  templateUrl: './event-comments.html',
})
export class EventComments {
  comments = input.required<CommentModel[]>();
  eventId = input.required<string>();
  organizerId = input<string>();
  limit = input<number>(4);
  compact = input<boolean>(false);

  commentCreated = output<CommentModel>();
  viewAll = output<void>();

  visibleComments = computed(() => this.comments().slice(-this.limit()));

  isHost(comment: CommentModel): boolean {
    return comment.userId.toLowerCase() === this.organizerId()?.toLowerCase();
  }
}
