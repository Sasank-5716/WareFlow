import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () =>
      import('./marketing/marketing.routes').then((module) => module.MARKETING_ROUTES),
  },
  {
    path: 'app',
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
