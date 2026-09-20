import { Component } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { Router } from '@angular/router';
import { Sidebar } from '../../components/sidebar/sidebar';
import { CardEvent } from '../../components/card-event/card-event';

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [Sidebar, CardEvent],
  templateUrl: 'events.html',
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