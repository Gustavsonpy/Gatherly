import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CommentModel, CreateCommentModel } from "./comment.model";

@Injectable({providedIn: 'root'})
export class CommentService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5008/api/comment';
    private apiUrlCreate = 'http://localhost:5008/api/comment/create';

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