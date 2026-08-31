import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Activity, DISTANCE_SPORTS, DURATION_SPORTS, IngestActivityRequest, SPORTS, SportName } from '../../models/activity.model';
import { ActivityService } from '../../services/activity.service';
import { AuthService } from '../../services/auth.service';

type MetricKind = 'distance' | 'duration' | 'steps';

@Component({
  selector: 'app-log-activity',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './log-activity.component.html',
  styleUrl: './log-activity.component.css',
})
export class LogActivityComponent {
  private activityService = inject(ActivityService);
  private auth = inject(AuthService);

  readonly sports = SPORTS;
  readonly currentUser = this.auth.currentUser;

  sport: SportName | '' = '';
  datetime = this.toDatetimeLocal(new Date());
  distance: number | null = null;
  duration = '';
  steps: number | null = null;

  submitting = signal(false);
  error = signal<string | null>(null);
  logged = signal<Activity | null>(null);

  metricKind(): MetricKind {
    if (this.sport === '') return 'steps';
    if (DISTANCE_SPORTS.includes(this.sport)) return 'distance';
    if (DURATION_SPORTS.includes(this.sport)) return 'duration';
    return 'steps';
  }

  onSportChange(): void {
    this.distance = null;
    this.duration = '';
    this.steps = null;
  }

  submit(): void {
    let isoDatetime: string;
    try {
      isoDatetime = new Date(this.datetime).toISOString();
    } catch {
      this.error.set('Enter a valid date/time.');
      return;
    }

    const payload: IngestActivityRequest = {
      datetime: isoDatetime,
    };

    if (this.sport) {
      payload.sport = this.sport;
    }

    const kind = this.metricKind();
    if (kind === 'distance') {
      if (this.distance == null) {
        this.error.set('Enter a distance in kilometers.');
        return;
      }
      payload.distance = this.distance;
    } else if (kind === 'duration') {
      if (!this.duration) {
        this.error.set("Enter a duration in 'minutes:seconds' format, e.g. 12:34.");
        return;
      }
      payload.duration = this.duration;
    } else {
      if (this.steps == null) {
        this.error.set('Enter a step count.');
        return;
      }
      payload.steps = this.steps;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.logged.set(null);

    this.activityService.ingest(payload).subscribe({
      next: (activity) => {
        this.logged.set(activity);
        this.submitting.set(false);
        this.distance = null;
        this.duration = '';
        this.steps = null;
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        this.error.set(err.error?.detail ?? 'Could not log this activity. Check the values and try again.');
      },
    });
  }

  private toDatetimeLocal(date: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }
}
