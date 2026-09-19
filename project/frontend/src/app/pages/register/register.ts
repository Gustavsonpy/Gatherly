import { Component, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { PurpleButton } from '../../components/buttons/purple-button';
import { GenericInput } from '../../components/input/generic-input/generic-input';
import { UserService } from '../../core/user/user.service';
import { applyFieldErrors } from '../../core/forms/field-errors.util';
import { passwordsMatchValidator } from '../../core/forms/validator';

interface RegisterForm {
  name: FormControl<string>,
  email: FormControl<string>,
  password: FormControl<string>,
  confirm_password: FormControl<string>,
  birthDate: FormControl<string>
}

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, PurpleButton, GenericInput],
  templateUrl: './register.html',
  styleUrl: './register.css',
})

export class Register {
    readonly form = new FormGroup<RegisterForm>({
      name: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(100)]
      }),
      email: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(100)]
      }),
      password: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(6)]
      }),
      confirm_password: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(6)]
      }),
      birthDate: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required]
      }),
    },
    { validators: passwordsMatchValidator }
  )
  
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor(private readonly user_service: UserService, private readonly router: Router){}

  submit(): void {
    if(this.form.invalid){
      this.form.markAllAsTouched();
      return;
    }

    const { name, email, password, birthDate } = this.form.getRawValue();
    this.loading.set(true);
    this.errorMessage.set(null);

    this.user_service.create({name, email, password, birthday: birthDate}).subscribe({
      next: () => this.router.navigate(['/login']),
      error: (err) => {
        const generalMessage = applyFieldErrors(this.form, err);
        this.errorMessage.set(generalMessage);
        this.loading.set(false);
      } 
    })
  }
}
