import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CreateEventModel, EventModel, ParticipantModel } from "./event.model";

@Injectable({providedIn: 'root'})
export class EventService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5008/api/event';

    getEvents(): Observable<EventModel[]> {
        return this.http.get<EventModel[]>(this.apiUrl);
    }

    getEventsByCity(): Observable<EventModel[]> {
        return this.http.get<EventModel[]>(`${this.apiUrl}/my-city`);
    }

    uploadImage(file: File): Observable<{ url: string }> {
        const formData = new FormData();
        formData.append('file', file);

        return this.http.post<{ url: string }>(
            `${this.apiUrl}/upload-image`,
            formData
        );
    }

    createEvent(event: CreateEventModel): Observable<EventModel> {
        return this.http.post<EventModel>(
            `${this.apiUrl}/create`,
            event
        );
    }

    getEventById(id: string): Observable<EventModel> {
        return this.http.get<EventModel>(`${this.apiUrl}/${id}`);
    }

    joinEvent(id: string): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${id}/join`, {});
    }

    leaveEvent(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}/join`);
    }

    getParticipants(id: string): Observable<ParticipantModel[]> {
        return this.http.get<ParticipantModel[]>(`${this.apiUrl}/${id}/participants`);
    }

    getMyEvents(): Observable<EventModel[]> {
        return this.http.get<EventModel[]>(`${this.apiUrl}/mine`);
    }

    cancelEvent(id: string): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${id}/cancel`, {});
    }
}