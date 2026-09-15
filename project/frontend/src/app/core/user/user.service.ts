import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { readonly } from "@angular/forms/signals";
import { environment } from "../../../environments/environment";
import { Observable } from "rxjs";

export interface UserDTO {
    id: string;
    name: string;
    email: string;
    birthday: string;
}

export interface CreateUSerDTO {
    name: string;
    email: string;
    password: string;
    birthday: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
    private readonly baseUrl = `${environment.apiUrl}/user`;
    
    constructor(private readonly http: HttpClient) {}

    create(dto: CreateUSerDTO): Observable<UserDTO>{
        return this.http.post<UserDTO>(`${this.baseUrl}/create`, dto);
    }
}

export function extractUserErrorMessage(err: HttpErrorResponse): string {
  const errors: string[] | undefined = err.error?.errors;
  if (errors?.length) {
    return errors.join(' ');
  }
  return 'Não foi possível criar sua conta. Tente novamente.';
}