
import { Component, input, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'generic-input',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="flex flex-col">
      <label [for]="id()">{{ label() }}</label>

      <div class="relative">
        <input
          [id]="id()"
          [type]="type() === 'password' && passwordVisible() ? 'text' : type()"
          [formControl]="control()"
          [autocomplete]="autoComplete()"
          [placeholder]="placeholder()"
          [class.date-input]="type() === 'date'"
          class="border border-primary-dark rounded-md p-2 pr-16 text-[12px] w-full"
        />

        @if (type() === 'password') {
          <button
            type="button"
            (click)="togglePasswordVisibility()"
            [attr.aria-label]="passwordVisible() ? 'Ocultar senha' : 'Mostrar senha'"
            [attr.aria-pressed]="passwordVisible()"
            class="absolute right-2 top-1/2 -translate-y-1/2 text-[12px] text-primary-dark cursor-pointer"
          >
            <img
              [src]="passwordVisible() ? '/icons/eye-off.svg' : '/icons/eye.svg'"
              alt=""
              class="w-4 h-4"
            />
          </button>
        }
      </div>

      <p
        class="text-[12px] text-red-500"
        [class.invisible]="!(control().invalid && control().touched)"
      >
        {{ control().errors?.['backend'] ?? errorMessage() }}
      </p>
    </div>
  `
})
export class GenericInput {
  id = input.required<string>();
  label = input.required<string>();
  control = input.required<FormControl<string | number>>();
  errorMessage = input<string>("Campo inválido");
  type = input<'text' | 'email' | 'password' | 'date' | 'time' | 'number'>('text');
  autoComplete = input<string>('off');
  placeholder = input<string>("");

  passwordVisible = signal(false);

  togglePasswordVisibility(): void {
    this.passwordVisible.update(value => !value);
  }
}
