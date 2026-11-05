import { Routes } from '@angular/router';

export const FEATURE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./workspace/workspace-page.component').then(
        (module) => module.WorkspacePageComponent,
      ),
  },
  {
    path: 'inventory',
    loadComponent: () =>
      import('../shared/pages/placeholder-page.component').then(
        (module) => module.PlaceholderPageComponent,
      ),
    data: {
      title: 'Inventory',
      description: 'Inventory operations will be implemented in this feature boundary.',
    },
  },
  {
    path: 'orders',
    loadComponent: () =>
      import('../shared/pages/placeholder-page.component').then(
        (module) => module.PlaceholderPageComponent,
      ),
    data: {
      title: 'Orders',
      description: 'Order operations will be implemented in this feature boundary.',
    },
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('../shared/pages/placeholder-page.component').then(
        (module) => module.PlaceholderPageComponent,
      ),
    data: {
      title: 'Reports & analytics',
      description: 'Operational reporting will be implemented in this feature boundary.',
    },
  },
  {
    path: 'settings',
    loadComponent: () =>
      import('../shared/pages/placeholder-page.component').then(
        (module) => module.PlaceholderPageComponent,
      ),
    data: {
      title: 'Settings',
      description: 'Tenant settings will be implemented in this feature boundary.',
    },
  },
];
