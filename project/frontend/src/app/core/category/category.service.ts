import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CategoryModel } from "./CategoryModel";

@Injectable({providedIn: 'root'})
export class CategoryService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5008/api/category';

    getCategories(): Observable<CategoryModel[]> {
        return this.http.get<CategoryModel[]>(this.apiUrl);
    }
}