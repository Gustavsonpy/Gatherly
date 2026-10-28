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
  {
    path: 'newEvent',
    loadComponent: () => 
      import('./pages/events/newEvent').then((n) => n.NewEvent),
  },
  {
    path: 'events/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/events/event-detail').then((m) => m.EventDetail),
  },
  {
    path: 'my-events',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/my-events/my-events').then((m) => m.MyEvents),
  },
  {
    path: 'homepage',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/homepage/homepage').then((h) => h.Homepage),
  },
  { path: '**', redirectTo: 'login' },
];