import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () =>
      import('./marketing/marketing.routes').then((module) => module.MARKETING_ROUTES),
  },
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then((module) => module.AUTH_ROUTES),
  },
  {
    path: 'app',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/features.routes').then((module) => module.FEATURE_ROUTES),
  },
  {
    path: '**',
    loadComponent: () =>
      import('./shared/components/error-page/error-page.component').then(
        (module) => module.ErrorPageComponent,
      ),
    data: { title: 'Page not found' },
  },
];
