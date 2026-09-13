import { Component, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { Router } from '@angular/router';
import { PurpleButton } from '../../../components/buttons/purple-button';
import { GenericInput } from '../../../components/input/generic-input/generic-input';

interface LoginForm {
  email: FormControl<string>;
  password: FormControl<string>;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, PurpleButton, GenericInput],
  templateUrl: './login.html',
  styleUrl: './login.css',
})

export class Login {
  readonly form = new FormGroup<LoginForm>({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.email, Validators.minLength(6)]
    }),
  });

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly currentYear = new Date().getFullYear();

  constructor(private readonly authService: AuthService, private readonly router: Router){}

  submit(): void {
    if(this.form.invalid){
      this.form.markAllAsTouched();
      return;
    }

    const { email, password } = this.form.getRawValue();
    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService.login(email, password).subscribe({
      next: () => this.router.navigate(['/events']),
      error: () => {
        console.log("Error");
        this.errorMessage.set('E-mail ou senha inválidos.');
        this.loading.set(false);
      },
    });
  }
}
