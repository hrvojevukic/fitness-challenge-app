import { Activity } from './activity.model';

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
  activities: Activity[];
  sportBreakdown: SportBreakdown[];
  dailyVolume: DailyVolume[];
}
