import { DatePipe } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'card-event',
  imports: [DatePipe],
  templateUrl: './card-event.html',
  styleUrl: './card-event.css',
})
export class CardEvent {
  private router = inject(Router);

  id = input<string>('');
  img = input<string>('');
  title = input<string>('');
  dateTimeStart = input<string>('');
  dateTimeEnd = input<string>('');
  localization = input<string>('');
  description = input<string>('');

  goToEvent(): void {
    this.router.navigate(['/events', this.id()]);
  }
}