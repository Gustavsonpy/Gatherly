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
  description: string | null;
  city: string | null;
  urlPhoto: string | null;
}

export interface UpdateUserDTO {
  name: string;
  birthday: string;
  description: string | null;
  city: string | null;
}

export interface UserPublicDTO {
  id: string;
  name: string;
  description: string | null;
  city: string | null;
  urlPhoto: string | null;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = `${environment.apiUrl}/user`;

  constructor(private readonly http: HttpClient) {}

  create(dto: CreateUserDTO): Observable<UserDTO> {
    return this.http.post<UserDTO>(`${this.baseUrl}/create`, dto);
  }

  getMe(): Observable<UserDTO> {
    return this.http.get<UserDTO>(`${this.baseUrl}/me`);
  }

  updateMe(dto: UpdateUserDTO): Observable<UserDTO> {
    return this.http.put<UserDTO>(`${this.baseUrl}/me`, dto);
  }

  getUserById(id: string): Observable<UserPublicDTO> {
    return this.http.get<UserPublicDTO>(`${this.baseUrl}/${id}`);
  } 
}