import { Component, OnInit, signal } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { Router } from '@angular/router';
import { Sidebar } from '../../components/sidebar/sidebar';
import { CardEvent } from '../../components/card-event/card-event';
import { EventService } from '../../core/event/event.service';
import { EventModel } from '../../core/event/event.model';

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [Sidebar, CardEvent],
  templateUrl: 'events.html',
})
export class Events implements OnInit{
  constructor(
    private readonly authService: AuthService,
    private readonly eventService: EventService,
    private readonly router: Router,
  ) {}

  events = signal<EventModel[]>([]);
  loading = signal(true);

  ngOnInit(): void {
    this.eventService.getEvents().subscribe({
      next: (data) => {
        this.events.set(data);
        console.log(`Events: ${this.events}`);
        this.loading.set(false);
      },
      error: (error) => {
        console.error(error);
        this.loading.set(false);
      }
    })
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}