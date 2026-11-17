import { Component, computed, inject, input, OnInit, output, signal } from '@angular/core';
import { Icon } from '../icon/icon';
import { AuthService } from '../../core/auth/auth.service';
import { EventModel, ParticipantModel } from '../../core/event/event.model';
import { GenericButton } from '../buttons/generic-button';
import { RateService } from '../../core/rating/rating.service';
import { CreateUserRatingModel } from '../../core/rating/user-rating.model';
import { DatePipe, TitleCasePipe } from '@angular/common';
import { ToastService } from '../../core/toaster/toaster.service';
import { ToasterErrorEventMessage, ToasterSuccessEventMessage } from '../../enum/toaster/toaster-enum';

@Component({
  selector: 'user-rating-modal',
  imports: [Icon, GenericButton, DatePipe, TitleCasePipe],
  templateUrl: './user-rating-modal.html',
})
export class RatingUserModal implements OnInit {
  private authService = inject(AuthService);
  private userRatingService = inject(RateService);
  private toastService = inject(ToastService);

  stars = [1, 2, 3, 4, 5];
  rate = ["Muito ruim", "Ruim", "Bom", "Muito bom", "Excelente"];

  ratedUser = input.required<ParticipantModel>();
  event = input.required<EventModel>();
  loading = signal(false);
  close = output<void>();
  rated = output<void>();
  starValue = signal<number>(0);
  rateFeedback = signal<string>("");
  errorMessage = signal<string>("");
  description = signal<string | null>("");

  activeRatingModal = signal<boolean>(false);

  private readonly userId = computed(() => this.authService.currentUser()?.id);

  ngOnInit(): void{
  }

  onDescriptionInput(event: Event): void {
    this.description.set((event.target as HTMLTextAreaElement).value);
  }

  submitRating(): void {
    this.loading.set(true);
    
    const rating: CreateUserRatingModel = {
      rate: this.starValue(),
      description: this.description(),
      ratedUserId: this.ratedUser().id,
      eventId: this.event().id
    }
    
    this.userRatingService.sendRating(rating).subscribe({
      next: () => {
        this.rated.emit();
        this.close.emit();
        this.toastService.success(ToasterSuccessEventMessage.rating_user);
      },
      error: (error) => {
        this.errorMessage.set(error.error?.errors?.[0] ?? 'Não foi possível avaliar o usuário.')
        this.toastService.error(ToasterErrorEventMessage.rating_user);
      }
    })
  }

  paintStars(index: number) {
    console.log(index);
    this.starValue.set(index);
    this.rateFeedback.set(this.rate[index - 1]);
  }
}
