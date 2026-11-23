import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CategoryModel } from "./CategoryModel";
import { environment } from "../../../environments/environment";

@Injectable({providedIn: 'root'})
export class CategoryService {
    private http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/category`;

    getCategories(): Observable<CategoryModel[]> {
        return this.http.get<CategoryModel[]>(this.apiUrl);
    }
}