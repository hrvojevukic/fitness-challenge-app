export const SPORT_COLORS: Record<string, string> = {
  running: '#2a78d6',
  walking: '#eb6834',
  cycling: '#1baf7a',
  gym: '#eda100',
  swimming: '#e87ba4',
  'Daily Steps': '#008300',
};

export const FALLBACK_SERIES_COLOR = '#4a3aa7';

export const ACCENT = '#2a78d6';

export function colorForLabel(label: string): string {
  return SPORT_COLORS[label] ?? SPORT_COLORS[label.toLowerCase()] ?? FALLBACK_SERIES_COLOR;
}
