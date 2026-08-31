import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../core/api-config';
import { LeaderboardEntry } from '../models/leaderboard.model';

@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private http = inject(HttpClient);
  private base = `${API_BASE_URL}/leaderboard`;

  get(): Observable<LeaderboardEntry[]> {
    return this.http.get<LeaderboardEntry[]>(this.base);
  }
}
