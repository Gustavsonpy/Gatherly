import { HttpErrorResponse } from '@angular/common/http';

export function getErrorMessage(
  err: unknown,
  fallback = 'Ocorreu um erro. Tente novamente.',
): string {
  if (!(err instanceof HttpErrorResponse)) return fallback;

  if (err.status === 0) {
    return 'Não foi possível conectar ao servidor. Verifique sua conexão.';
  }

  const errors = err.error?.errors;
  if (Array.isArray(errors) && errors.length > 0) {
    return errors.join(' ');
  }

  return fallback;
}