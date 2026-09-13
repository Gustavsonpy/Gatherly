import { Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'generic-input',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="flex flex-col">
      <label [for]="id()">{{ label() }}</label>
      <input
        [id]="id()"
        [type]="type()"
        [formControl]="control()"
        [autocomplete]="autoComplete()"
        [placeholder]="placeholder()"
        class="border border-[#5B21B6] rounded-md p-2 text-[12px]"
      />
      @if (control().invalid && control().touched) {
        <p class="text-red-500">{{ errorMessage() }}</p>
      }
    </div>
  `
})
export class GenericInput {
  id = input.required<string>();
  label = input.required<string>();
  control = input.required<FormControl<string>>();
  errorMessage = input<string>("Campo inválido");
  type = input<'text' | 'email' | 'password'>('text');
  autoComplete = input<string>('off');
  placeholder = input<string>("");
}
