import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CommentModel, CreateCommentModel } from "./comment.model";
import { environment } from "../../../environments/environment";

@Injectable({providedIn: 'root'})
export class CommentService {
    private http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/comment`;
    private readonly apiUrlCreate = `${this.apiUrl}/create`;

    getComments(): Observable<CommentModel[]> {
        return this.http.get<CommentModel[]>(this.apiUrl);
    }

    getCommentsByEvent(eventId: string): Observable<CommentModel[]> {
        return this.http.get<CommentModel[]>(this.apiUrl + `/${eventId}`);
    }

    createComment(data: CreateCommentModel): Observable<CommentModel> {
        return this.http.post<CommentModel>(this.apiUrlCreate, data);
    }
}