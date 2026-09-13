import { Component } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-events',
  standalone: true,
  template: `
    <div class="min-h-dvh flex flex-col items-center justify-center gap-4 bg-[#F4F1E8] text-[#14211A]">
      <p class="font-serif text-2xl">Você está autenticado.</p>
      <button
        (click)="logout()"
        class="rounded-md border border-[#1F3A2E] px-4 py-2 text-sm text-[#1F3A2E] hover:bg-[#1F3A2E] hover:text-[#F4F1E8]"
      >
        Sair
      </button>
    </div>
  `,
})
export class Events {
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}