import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/** Libera a rota apenas para administradores; os demais voltam para a home. */
export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAdmin()) {
    return true;
  }

  void router.navigate([authService.isAuthenticated() ? '/homepage' : '/login']);
  return false;
};
