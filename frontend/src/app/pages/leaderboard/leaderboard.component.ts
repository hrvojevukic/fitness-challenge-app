import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { LeaderboardEntry } from '../../models/leaderboard.model';
import { LeaderboardService } from '../../services/leaderboard.service';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.css',
})
export class LeaderboardComponent implements OnInit {
  private leaderboardService = inject(LeaderboardService);
  private router = inject(Router);

  entries = signal<LeaderboardEntry[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  openDashboard(userId: number): void {
    this.router.navigate(['/dashboard', userId]);
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.leaderboardService.get().subscribe({
      next: (entries) => {
        this.entries.set(entries);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load the leaderboard. Is the API running?');
        this.loading.set(false);
      },
    });
  }

  trendLabel(entry: LeaderboardEntry): string {
    switch (entry.trend) {
      case 'up':
        return `▲ up from #${entry.previousRank}`;
      case 'down':
        return `▼ down from #${entry.previousRank}`;
      case 'same':
        return '— steady';
      default:
        return '★ new';
    }
  }
}
