import { Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

export interface CreateUserDTO {
  name: string;
  email: string;
  password: string;
  birthday: string;
}

export interface UserDTO {
  id: string;
  name: string;
  email: string;
  birthday: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = `${environment.apiUrl}/user`;

  constructor(private readonly http: HttpClient) {}

  create(dto: CreateUserDTO): Observable<UserDTO> {
    return this.http.post<UserDTO>(`${this.baseUrl}/create`, dto);
  }
}