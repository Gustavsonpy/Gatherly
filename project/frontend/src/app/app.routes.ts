import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./pages/register/register').then((r) => r.Register),
  },
  {
    path: 'events',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/events/events').then((m) => m.Events),
  },
  { path: '**', redirectTo: 'login' },
];