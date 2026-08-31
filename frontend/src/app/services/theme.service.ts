import { Injectable, effect, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const THEME_KEY = 'fc.theme';
const FONT_STEP_KEY = 'fc.fontStep2';

const FONT_STEPS = [0.75, 1, 1.15, 1.3];
const NORMAL_FONT_STEP = 1;

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly maxFontStep = FONT_STEPS.length - 1;

  readonly theme = signal<ThemeMode>(this.readTheme());
  readonly fontStep = signal<number>(this.readFontStep());

  constructor() {
    effect(() => {
      const theme = this.theme();
      const step = this.fontStep();
      const root = document.documentElement;
      root.setAttribute('data-theme', theme);
      root.style.setProperty('--font-scale', String(FONT_STEPS[step] ?? 1));
      try {
        localStorage.setItem(THEME_KEY, theme);
        localStorage.setItem(FONT_STEP_KEY, String(step));
      } catch {
        return;
      }
    });
  }

  toggleTheme(): void {
    this.theme.update((t) => (t === 'dark' ? 'light' : 'dark'));
  }

  increaseFont(): void {
    this.fontStep.update((s) => Math.min(s + 1, this.maxFontStep));
  }

  decreaseFont(): void {
    this.fontStep.update((s) => Math.max(s - 1, 0));
  }

  setFontStep(step: number | string): void {
    const n = Math.round(Number(step));
    if (!Number.isFinite(n)) {
      return;
    }
    this.fontStep.set(Math.min(Math.max(n, 0), this.maxFontStep));
  }

  private readTheme(): ThemeMode {
    try {
      const stored = localStorage.getItem(THEME_KEY);
      if (stored === 'light' || stored === 'dark') {
        return stored;
      }
    } catch {
      return 'light';
    }
    try {
      return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    } catch {
      return 'light';
    }
  }

  private readFontStep(): number {
    try {
      const raw = localStorage.getItem(FONT_STEP_KEY);
      if (raw !== null && raw !== '') {
        const n = Number(raw);
        if (Number.isInteger(n) && n >= 0 && n <= this.maxFontStep) {
          return n;
        }
      }
    } catch {
      return NORMAL_FONT_STEP;
    }
    return NORMAL_FONT_STEP;
  }
}
