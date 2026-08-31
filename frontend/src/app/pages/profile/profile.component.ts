import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Dashboard } from '../../models/dashboard.model';
import { ActivityService } from '../../services/activity.service';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { DashboardViewComponent } from '../../shared/dashboard-view/dashboard-view.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, DashboardViewComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit {
  private auth = inject(AuthService);
  private userService = inject(UserService);
  private activityService = inject(ActivityService);
  private router = inject(Router);

  readonly user = this.auth.currentUser;

  dashboard = signal<Dashboard | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading.set(true);
    this.userService.getMyDashboard().subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load your dashboard.');
        this.loading.set(false);
      },
    });
  }

  onActivityDeleted(activityId: number): void {
    this.error.set(null);
    this.activityService.remove(activityId).subscribe({
      next: () => this.loadDashboard(),
      error: () => this.error.set('Could not delete that activity. Try again.'),
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
