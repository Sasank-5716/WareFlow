import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const publicAuthGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.isAuthenticated
    ? inject(Router).createUrlTree(['/app'])
    : true;
};
