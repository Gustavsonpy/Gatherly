import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

const TOKEN_KEY = 'gatherly_token';

interface LoginResponse {
  token: string;
}

export interface CurrentUser {
  id: string;
  email: string;
  name: string;
  isAdmin: boolean;
}

const NAME_IDENTIFIER_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
export const ADMIN_ROLE = 'Admin';

/** A claim de role pode vir como string (um papel) ou array (vários papéis). */
function hasRole(payload: Record<string, unknown>, role: string): boolean {
  const value = payload[ROLE_CLAIM];
  return Array.isArray(value) ? value.includes(role) : value === role;
}

function decodeUserFromToken(token: string): CurrentUser | null {
  try {
    const payloadBase64 = token.split('.')[1];
    const payloadJson = atob(payloadBase64.replace(/-/g, '+').replace(/_/g, '/'));
    const payload = JSON.parse(payloadJson);

    return {
      id: payload[NAME_IDENTIFIER_CLAIM],
      email: payload[EMAIL_CLAIM],
      name: payload['name'],
      isAdmin: hasRole(payload, ADMIN_ROLE),
    };
  } catch {
    return null;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenSignal = signal<string | null>(
    localStorage.getItem(TOKEN_KEY),
  );

  readonly isAuthenticated = computed(() => !!this.tokenSignal());

  readonly currentUser = computed<CurrentUser | null>(() => {
    const token = this.tokenSignal();
    return token ? decodeUserFromToken(token) : null;
  });

  /** Só controla o que aparece na tela; quem decide o acesso de verdade é o backend. */
  readonly isAdmin = computed(() => this.currentUser()?.isAdmin ?? false);

  constructor(private readonly http: HttpClient) {}

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/login`, { email, password })
      .pipe(tap(({ token }) => this.setToken(token)));
  }

  logout(): void {
    this.setToken(null);
  }

  getToken(): string | null {
    return this.tokenSignal();
  }

  private setToken(token: string | null): void {
    this.tokenSignal.set(token);
    if (token) {
      localStorage.setItem(TOKEN_KEY, token);
    } else {
      localStorage.removeItem(TOKEN_KEY);
    }
  }
}