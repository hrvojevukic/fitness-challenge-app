export interface SportBreakdown {
  label: string;
  activityCount: number;
  totalPoints: number;
}

export interface DailyVolume {
  date: string;
  points: number;
  activityCount: number;
}

export interface Dashboard {
  userId: number;
  fullName: string;
  totalPoints: number;
  activityCount: number;
  sportBreakdown: SportBreakdown[];
  dailyVolume: DailyVolume[];
}
