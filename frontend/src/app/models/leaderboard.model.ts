export type Trend = 'up' | 'down' | 'same' | 'new';

export interface LeaderboardEntry {
  userId: number;
  fullName: string;
  totalPoints: number;
  rank: number;
  previousRank: number | null;
  trend: Trend;
}
