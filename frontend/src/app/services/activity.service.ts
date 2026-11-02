import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../core/api-config';
import { Activity, IngestActivityRequest } from '../models/activity.model';
import { Page } from '../models/page.model';

@Injectable({ providedIn: 'root' })
export class ActivityService {
  private http = inject(HttpClient);
  private base = `${API_BASE_URL}/activities`;

  getForUser(userId: number, page: number, pageSize: number): Observable<Page<Activity>> {
    return this.http.get<Page<Activity>>(this.base, { params: { userId, page, pageSize } });
  }

  ingest(payload: IngestActivityRequest): Observable<Activity> {
    return this.http.post<Activity>(this.base, payload);
  }

  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
