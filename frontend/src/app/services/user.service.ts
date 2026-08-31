import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../core/api-config';
import { Dashboard } from '../models/dashboard.model';
import { User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private base = `${API_BASE_URL}/users`;

  register(firstName: string, lastName: string): Observable<User> {
    return this.http.post<User>(this.base, { firstName, lastName });
  }

  getAll(): Observable<User[]> {
    return this.http.get<User[]>(this.base);
  }

  getDashboard(userId: number): Observable<Dashboard> {
    return this.http.get<Dashboard>(`${this.base}/${userId}/dashboard`);
  }

  getMyDashboard(): Observable<Dashboard> {
    return this.http.get<Dashboard>(`${API_BASE_URL}/me/dashboard`);
  }
}
