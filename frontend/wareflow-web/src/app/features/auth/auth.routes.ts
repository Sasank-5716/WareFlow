import { Routes } from '@angular/router';
import { publicAuthGuard } from '../../core/auth/public-auth.guard';

export const AUTH_ROUTES: Routes = [
  {
    path: '',
    canActivate: [publicAuthGuard],
    loadComponent: () =>
      import('./auth-shell.component').then((module) => module.AuthShellComponent),
    children: [
      {
        path: 'login',
        loadComponent: () =>
          import('./login-page.component').then((module) => module.LoginPageComponent),
      },
      {
        path: 'register',
        loadComponent: () =>
          import('./register-page.component').then(
            (module) => module.RegisterPageComponent,
          ),
      },
      {
        path: 'verify-email',
        loadComponent: () =>
          import('./verify-email-page.component').then(
            (module) => module.VerifyEmailPageComponent,
          ),
      },
      {
        path: 'forgot-password',
        loadComponent: () =>
          import('./forgot-password-page.component').then(
            (module) => module.ForgotPasswordPageComponent,
          ),
      },
      {
        path: 'reset-password',
        loadComponent: () =>
          import('./reset-password-page.component').then(
            (module) => module.ResetPasswordPageComponent,
          ),
      },
      {
        path: 'session-expired',
        loadComponent: () =>
          import('./session-expired-page.component').then(
            (module) => module.SessionExpiredPageComponent,
          ),
      },
      { path: '', pathMatch: 'full', redirectTo: 'login' },
    ],
  },
];
