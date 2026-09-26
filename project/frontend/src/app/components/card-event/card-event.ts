import { Component, input } from '@angular/core';

@Component({
  selector: 'card-event',
  imports: [],
  templateUrl: './card-event.html',
  styleUrl: './card-event.css',
})

export class CardEvent {
  img = input<string>('');
  title = input<string>('');
  time = input<string>('');
  localization = input<string>('');
  description = input<string>('');
}
