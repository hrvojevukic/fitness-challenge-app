import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { LeaderboardEntry } from '../../models/leaderboard.model';
import { LeaderboardService } from '../../services/leaderboard.service';
import { PagerComponent } from '../../shared/pager/pager.component';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule, RouterLink, PagerComponent],
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.css',
})
export class LeaderboardComponent implements OnInit {
  private leaderboardService = inject(LeaderboardService);
  private router = inject(Router);

  entries = signal<LeaderboardEntry[]>([]);
  page = signal(1);
  totalPages = signal(0);
  loading = signal(true);
  error = signal<string | null>(null);

  openDashboard(userId: number): void {
    this.router.navigate(['/dashboard', userId]);
  }

  ngOnInit(): void {
    this.load();
  }

  load(page = this.page()): void {
    this.loading.set(true);
    this.error.set(null);
    this.leaderboardService.get(page, PAGE_SIZE).subscribe({
      next: (result) => {
        this.entries.set(result.items);
        this.page.set(result.page);
        this.totalPages.set(result.totalPages);
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
