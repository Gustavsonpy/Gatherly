import { AbstractControl, ValidationErrors } from '@angular/forms';

export function passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirmControl = group.get('confirm_password');

  if (!confirmControl) return null;

  if (password !== confirmControl.value) {
    confirmControl.setErrors({ ...confirmControl.errors, mismatch: true });
  } else if (confirmControl.hasError('mismatch')) {
    const { mismatch, ...rest } = confirmControl.errors ?? {};
    confirmControl.setErrors(Object.keys(rest).length ? rest : null);
  }

  return null;
}