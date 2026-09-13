import { Component, input, output } from '@angular/core';

@Component({
  selector: 'purple-button',
  standalone: true,
  template: `
    <button
      [type]="type()"
      [disabled]="disabled()"
      (click)="clicked.emit()"
      class="rounded-md bg-[#7C3AED] text-sm text-white p-2 w-full cursor-pointer"
    >
    <ng-content />
    </button>
  `
})
export class PurpleButton {
  type = input<'button' | 'submit'>('button');
  disabled = input(false);
  clicked = output<void>();
}
