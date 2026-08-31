import { CommonModule } from '@angular/common';
import { Component, computed, inject, input, output } from '@angular/core';
import { ChartData, TooltipItem } from 'chart.js';
import { Dashboard, DailyVolume } from '../../models/dashboard.model';
import { ThemeService } from '../../services/theme.service';
import { ChartComponent } from '../chart/chart.component';
import { ACCENT, colorForLabel } from '../chart/chart-palette';

const DAY_MS = 86_400_000;
const MAX_TIMELINE_DAYS = 60;

const CHART_INK = {
  light: { label: '#898781', grid: '#e1e0d9', legend: '#52514e', segmentBorder: '#fcfcfb' },
  dark: { label: '#9aa3b4', grid: '#2b303c', legend: '#c8ccd6', segmentBorder: '#1b1e26' },
};

@Component({
  selector: 'app-dashboard-view',
  standalone: true,
  imports: [CommonModule, ChartComponent],
  templateUrl: './dashboard-view.component.html',
  styleUrl: './dashboard-view.component.css',
})
export class DashboardViewComponent {
  dashboard = input.required<Dashboard>();
  canDelete = input(false);
  activityDeleted = output<number>();

  private themeService = inject(ThemeService);
  private ink = computed(() => CHART_INK[this.themeService.theme()]);

  private volumeTimeline = computed(() => {
    const d = this.dashboard();
    if (!d || d.dailyVolume.length === 0) {
      return null;
    }
    return buildTimeline(d.dailyVolume);
  });

  volumeChartData = computed<ChartData | null>(() => {
    const tl = this.volumeTimeline();
    if (!tl) {
      return null;
    }
    const dense = tl.points.length > 40;
    return {
      labels: tl.labels,
      datasets: [
        {
          label: 'Points',
          data: tl.points,
          borderColor: ACCENT,
          backgroundColor: 'rgba(42, 120, 214, 0.14)',
          fill: true,
          tension: 0.35,
          borderWidth: 2,
          pointRadius: dense ? 0 : 3,
          pointHoverRadius: 5,
          pointBackgroundColor: ACCENT,
        },
      ],
    };
  });

  volumeChartOptions = computed(() => {
    const ink = this.ink();
    const counts = this.volumeTimeline()?.counts ?? [];
    return {
      interaction: { mode: 'index' as const, intersect: false },
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            label: (ctx: TooltipItem<'line'>) => {
              const pts = ctx.parsed.y;
              const n = counts[ctx.dataIndex] ?? 0;
              return n > 0
                ? `${pts} pts · ${n} ${n === 1 ? 'activity' : 'activities'}`
                : `${pts} pts`;
            },
          },
        },
      },
      scales: {
        x: {
          grid: { display: false },
          ticks: { color: ink.label, autoSkip: true, maxTicksLimit: 8, maxRotation: 0 },
        },
        y: {
          beginAtZero: true,
          grid: { color: ink.grid },
          ticks: { color: ink.label, precision: 0 },
        },
      },
    };
  });

  sportBreakdownData = computed<ChartData | null>(() => {
    const d = this.dashboard();
    if (!d || d.sportBreakdown.length === 0) {
      return null;
    }
    return {
      labels: d.sportBreakdown.map((s) => this.formatLabel(s.label)),
      datasets: [
        {
          data: d.sportBreakdown.map((s) => s.totalPoints),
          backgroundColor: d.sportBreakdown.map((s) => colorForLabel(s.label)),
          borderColor: this.ink().segmentBorder,
          borderWidth: 2,
        },
      ],
    };
  });

  sportBreakdownOptions = computed(() => ({
    plugins: {
      legend: { position: 'bottom' as const, labels: { color: this.ink().legend, boxWidth: 12 } },
      tooltip: {
        callbacks: {
          title: () => '',
          label: (ctx: TooltipItem<'doughnut'>) => {
            const value = ctx.parsed;
            const total = (ctx.dataset.data as number[]).reduce((a, b) => a + b, 0);
            const pct = total ? Math.round((value / total) * 100) : 0;
            return `${ctx.label}  ${value} (${pct}%)`;
          },
        },
      },
    },
  }));

  activityCountData = computed<ChartData | null>(() => {
    const d = this.dashboard();
    if (!d || d.sportBreakdown.length === 0) {
      return null;
    }
    return {
      labels: d.sportBreakdown.map((s) => this.formatLabel(s.label)),
      datasets: [
        {
          label: 'Activities logged',
          data: d.sportBreakdown.map((s) => s.activityCount),
          backgroundColor: d.sportBreakdown.map((s) => colorForLabel(s.label)),
          borderRadius: 4,
          maxBarThickness: 36,
        },
      ],
    };
  });

  activityCountOptions = computed(() => {
    const ink = this.ink();
    return {
      indexAxis: 'y' as const,
      plugins: { legend: { display: false } },
      scales: {
        x: {
          beginAtZero: true,
          grid: { color: ink.grid },
          ticks: { color: ink.label, precision: 0 },
        },
        y: { grid: { display: false }, ticks: { color: ink.label } },
      },
    };
  });

  requestDelete(activity: Dashboard['activities'][number]): void {
    const what = `${this.formatLabel(activity.sport ?? 'Daily Steps')} · ${this.formatValue(activity)}`;
    if (confirm(`Delete this activity?\n\n${what}\n\nThis can't be undone.`)) {
      this.activityDeleted.emit(activity.id);
    }
  }

  formatLabel(label: string): string {
    return label === 'Daily Steps' ? label : label.charAt(0).toUpperCase() + label.slice(1);
  }

  formatValue(activity: Dashboard['activities'][number]): string {
    if (activity.distance != null) {
      return `${activity.distance} km`;
    }
    if (activity.duration != null) {
      return `${activity.duration} min`;
    }
    if (activity.steps != null) {
      return `${activity.steps} steps`;
    }
    return '—';
  }
}

interface Timeline {
  labels: string[];
  points: number[];
  counts: number[];
}

function buildTimeline(volume: DailyVolume[]): Timeline {
  const byDay = new Map<string, { points: number; count: number }>();
  for (const v of volume) {
    byDay.set(v.date.slice(0, 10), { points: v.points, count: v.activityCount });
  }

  const days = [...byDay.keys()].sort();
  let startMs = Date.parse(days[0] + 'T00:00:00Z');
  let endMs = Date.parse(days[days.length - 1] + 'T00:00:00Z');

  if (startMs === endMs) {
    startMs -= 3 * DAY_MS;
    endMs += 3 * DAY_MS;
  }
  const maxSpanMs = (MAX_TIMELINE_DAYS - 1) * DAY_MS;
  if (endMs - startMs > maxSpanMs) {
    startMs = endMs - maxSpanMs;
  }

  const multiYear = new Date(startMs).getUTCFullYear() !== new Date(endMs).getUTCFullYear();
  const fmt = new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    timeZone: 'UTC',
    ...(multiYear ? { year: '2-digit' } : {}),
  });

  const labels: string[] = [];
  const points: number[] = [];
  const counts: number[] = [];
  for (let t = startMs; t <= endMs; t += DAY_MS) {
    const hit = byDay.get(new Date(t).toISOString().slice(0, 10));
    labels.push(fmt.format(t));
    points.push(hit?.points ?? 0);
    counts.push(hit?.count ?? 0);
  }
  return { labels, points, counts };
}
