import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CreateUserRatingModel } from './user-rating.model';

@Injectable({providedIn: 'root'})
export class RateService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5008/api';
  private ratingUrl = `${this.apiUrl}/rate`;

  sendRating(rate: CreateUserRatingModel) {
    return this.http.post<CreateUserRatingModel>(`${this.ratingUrl}/create`, rate);
  }
}