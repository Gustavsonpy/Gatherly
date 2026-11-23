import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CreateUserRatingModel, UserScore } from './user-rating.model';
import { environment } from '../../../environments/environment';

@Injectable({providedIn: 'root'})
export class RateService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}`;
  private readonly ratingUrl = `${this.apiUrl}/rate`;

  sendRating(rate: CreateUserRatingModel) {
    return this.http.post<CreateUserRatingModel>(`${this.ratingUrl}/create`, rate);
  }

  getUserScore(userId: string) {
    return this.http.get<UserScore>(`${userId}/score`);
  }
}