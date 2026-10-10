import { Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'generic-button',
  standalone: true,
  template: `
    <button
      [type]="type()"
      [disabled]="disabled()"
      (click)="clicked.emit()"
      [class]="classes()"
    >
      <ng-content />
    </button>
  `
})
export class GenericButton {
  type = input<'button' | 'submit'>('button');
  disabled = input(false);
  variant = input<'primary' | 'secondary'>('primary');
  clicked = output<void>();

  private readonly base =
    'rounded-md border text-sm p-2 w-full cursor-pointer transition-colors disabled:opacity-50 disabled:cursor-not-allowed';

  private readonly variants = {
    primary: 'bg-[#7C3AED] border-[#7C3AED] text-white hover:bg-[#6D28D9]',
    secondary: 'bg-white border-[#5B21B6] text-[#5B21B6] hover:bg-[#EDE9FE]',
  };

  classes = computed(() => `${this.base} ${this.variants[this.variant()]}`);
}
