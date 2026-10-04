import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';

@Component({
  selector: 'card-event',
  imports: [DatePipe],
  templateUrl: './card-event.html',
  styleUrl: './card-event.css',
})

export class CardEvent {
  img = input<string>('');
  title = input<string>('');
  time = input<string>('');
  date = input<string>('');
  localization = input<string>('');
  description = input<string>('');
}
