export type SportName = 'running' | 'walking' | 'cycling' | 'gym' | 'swimming';

export const SPORTS: SportName[] = ['running', 'walking', 'cycling', 'gym', 'swimming'];

export const DISTANCE_SPORTS: SportName[] = ['running', 'walking', 'cycling'];
export const DURATION_SPORTS: SportName[] = ['gym', 'swimming'];

export interface Activity {
  id: number;
  userId: number;
  datetime: string;
  sport: SportName | null;
  steps: number | null;
  distance: number | null;
  duration: string | null;
  points: number;
}

export interface IngestActivityRequest {
  userId?: string;
  datetime: string;
  sport?: SportName;
  steps?: number;
  distance?: number;
  duration?: string;
}
