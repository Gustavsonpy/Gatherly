import { Component, inject, input, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommentService } from '../../core/comments/comment.service';
import { CommentModel } from '../../core/comments/comment.model';

@Component({
  selector: 'commentSender',
  imports: [ReactiveFormsModule],
  templateUrl: './comment-sender.html',
  styleUrl: './comment-sender.css',
})

export class CommentSender {
  private commentService = inject(CommentService);

  eventId = input.required<string>();
  commentCreated = output<CommentModel>();

  sending = signal(false);

  commentControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required]
  });

  send(): void {
    const text = this.commentControl.value.trim();

    if (!text || this.sending()) return;

    this.sending.set(true);

    this.commentService.createComment({text, eventId: this.eventId() }).subscribe({
      next: (created) => {
        this.commentCreated.emit(created);
        this.commentControl.reset();
        this.sending.set(false);
      },
      error: (err) => {
        console.error(err);
        this.sending.set(false);
      },
    });
  }
}
