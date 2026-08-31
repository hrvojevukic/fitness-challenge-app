import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../core/api-config';
import { Activity, IngestActivityRequest } from '../models/activity.model';

@Injectable({ providedIn: 'root' })
export class ActivityService {
  private http = inject(HttpClient);
  private base = `${API_BASE_URL}/activities`;

  ingest(payload: IngestActivityRequest): Observable<Activity> {
    return this.http.post<Activity>(this.base, payload);
  }

  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
