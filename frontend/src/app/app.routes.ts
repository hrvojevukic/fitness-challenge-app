import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'leaderboard' },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'leaderboard',
    loadComponent: () => import('./pages/leaderboard/leaderboard.component').then((m) => m.LeaderboardComponent),
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/profile/profile.component').then((m) => m.ProfileComponent),
  },
  {
    path: 'dashboard/:id',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: 'log-activity',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/log-activity/log-activity.component').then((m) => m.LogActivityComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./pages/register-user/register-user.component').then((m) => m.RegisterUserComponent),
  },
  { path: '**', redirectTo: 'leaderboard' },
];
