import { Routes } from '@angular/router';

export const MARKETING_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../shared/pages/placeholder-page.component').then(
        (module) => module.PlaceholderPageComponent,
      ),
    data: {
      title: 'Welcome to WAREFLOW',
      description: 'A focused foundation for connected warehouse operations.',
    },
  },
  {
    path: 'about',
    loadComponent: () =>
      import('../shared/pages/placeholder-page.component').then(
        (module) => module.PlaceholderPageComponent,
      ),
    data: {
      title: 'About WAREFLOW',
      description: 'The public marketing experience stays separate from the operational application.',
    },
  },
];
