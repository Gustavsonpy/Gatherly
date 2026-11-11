import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { Icon } from '../icon/icon';
import { AuthService } from '../../core/auth/auth.service';
import { ParticipantModel } from '../../core/event/event.model';
import { GenericButton } from '../buttons/generic-button';

@Component({
  selector: 'user-rating-modal',
  imports: [Icon, GenericButton],
  templateUrl: './user-rating-modal.html',
})
export class RatingUserModal implements OnInit {
  private authService = inject(AuthService);

  ratedUser = input.required<ParticipantModel>();
  eventId = input.required<string>();

  activeRatingModal = signal<boolean>(false);

  private readonly userId = computed(() => this.authService.currentUser()?.id);

  ngOnInit(): void{

  }
}
