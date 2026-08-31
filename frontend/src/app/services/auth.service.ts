import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_BASE_URL } from '../core/api-config';
import { LoginResponse } from '../models/auth.model';
import { User } from '../models/user.model';

const TOKEN_KEY = 'fc.token';
const USER_KEY = 'fc.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private base = `${API_BASE_URL}/auth`;

  private _token = signal<string | null>(this.readToken());
  private _user = signal<User | null>(this.readUser());

  readonly currentUser = this._user.asReadonly();
  readonly isLoggedIn = computed(() => this._token() !== null && this._user() !== null);

  get token(): string | null {
    return this._token();
  }

  login(firstName: string, lastName: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.base}/login`, { firstName, lastName }).pipe(
      tap((res) => this.setSession(res)),
    );
  }

  logout(): void {
    this._token.set(null);
    this._user.set(null);
    try {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
    } catch {
      return;
    }
  }

  private setSession(res: LoginResponse): void {
    this._token.set(res.token);
    this._user.set(res.user);
    try {
      localStorage.setItem(TOKEN_KEY, res.token);
      localStorage.setItem(USER_KEY, JSON.stringify(res.user));
    } catch {
      return;
    }
  }

  private readToken(): string | null {
    try {
      return localStorage.getItem(TOKEN_KEY);
    } catch {
      return null;
    }
  }

  private readUser(): User | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? (JSON.parse(raw) as User) : null;
    } catch {
      return null;
    }
  }
}
