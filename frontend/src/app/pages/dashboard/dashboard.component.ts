import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Dashboard } from '../../models/dashboard.model';
import { User } from '../../models/user.model';
import { ActivityService } from '../../services/activity.service';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { DashboardViewComponent } from '../../shared/dashboard-view/dashboard-view.component';

const USER_PICKER_SIZE = 100;

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, DashboardViewComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private userService = inject(UserService);
  private activityService = inject(ActivityService);
  private auth = inject(AuthService);
  private destroyRef = inject(DestroyRef);

  users = signal<User[]>([]);
  selectedUserId = signal<number | null>(null);
  dashboard = signal<Dashboard | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  readonly canDelete = computed(() => this.auth.currentUser()?.id === this.selectedUserId());

  ngOnInit(): void {
    this.userService.getPage(1, USER_PICKER_SIZE).subscribe({
      next: (result) => {
        const users = result.items;
        this.users.set(users);
        if (this.route.snapshot.paramMap.get('id') === null && this.selectedUserId() === null && users[0]) {
          this.selectUser(users[0].id);
        }
      },
      error: () => this.error.set('Could not load users. Is the API running?'),
    });

    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const routeId = params.get('id');
      if (routeId && Number(routeId) !== this.selectedUserId()) {
        this.selectUser(Number(routeId));
      }
    });
  }

  selectUser(userId: number): void {
    this.selectedUserId.set(userId);
    this.loading.set(true);
    this.error.set(null);
    this.userService.getDashboard(userId).subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load this user\'s dashboard.');
        this.loading.set(false);
      },
    });
  }

  onUserPicked(event: Event): void {
    const id = Number((event.target as HTMLSelectElement).value);
    if (id) {
      this.selectUser(id);
    }
  }

  onActivityDeleted(activityId: number): void {
    this.error.set(null);
    this.activityService.remove(activityId).subscribe({
      next: () => {
        const id = this.selectedUserId();
        if (id) {
          this.selectUser(id);
        }
      },
      error: () => this.error.set('Could not delete that activity. Try again.'),
    });
  }
}
