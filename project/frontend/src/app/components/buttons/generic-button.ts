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
    primary: 'bg-primary border-primary text-white hover:bg-primary-hover',
    secondary: 'bg-white border-primary-dark text-primary-dark hover:bg-primary-light',
  };

  classes = computed(() => `${this.base} ${this.variants[this.variant()]}`);
}
