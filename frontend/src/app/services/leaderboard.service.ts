import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../core/api-config';
import { LeaderboardEntry } from '../models/leaderboard.model';
import { Page } from '../models/page.model';

@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private http = inject(HttpClient);
  private base = `${API_BASE_URL}/leaderboard`;

  get(page: number, pageSize: number): Observable<Page<LeaderboardEntry>> {
    return this.http.get<Page<LeaderboardEntry>>(this.base, { params: { page, pageSize } });
  }
}
