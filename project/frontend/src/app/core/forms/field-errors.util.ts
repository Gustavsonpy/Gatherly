import { FormGroup } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

export interface FieldErrorsResponse {
  errors?: string[];
  fieldErrors?: Record<string, string[]>;
}

export function applyFieldErrors(form: FormGroup, err: HttpErrorResponse, fieldNameMap: Record<string, string> = {},): string | null {
  const body: FieldErrorsResponse | undefined = err.error;
  const fieldErrors = body?.fieldErrors;

  if (!fieldErrors || Object.keys(fieldErrors).length === 0) {
    return body?.errors?.join(' ') ?? 'Ocorreu um erro. Tente novamente.';
  }

  let unmatched: string[] = [];

  for (const [backendField, messages] of Object.entries(fieldErrors)) {
    const controlName = fieldNameMap[backendField] ?? backendField;
    const control = form.get(controlName);

    if (control) {
      control.setErrors({ backend: messages.join(' ') });
      control.markAsTouched();
    } else {
      unmatched.push(...messages);
    }
  }

  return unmatched.length ? unmatched.join(' ') : null;
}