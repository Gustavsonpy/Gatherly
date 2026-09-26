import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { EventModel } from "./event.model";

@Injectable({providedIn: 'root'})
export class EventService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5008/api/event';

    getEvents(): Observable<EventModel[]> {
        return this.http.get<EventModel[]>(this.apiUrl);
    }
}