import { User } from './user.model';

export interface LoginRequest {
  firstName: string;
  lastName: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}
