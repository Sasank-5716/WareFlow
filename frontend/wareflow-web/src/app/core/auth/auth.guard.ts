import { inject } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { CanActivateFn, Router } from '@angular/router';
import { filter, map, take } from 'rxjs';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.authState() !== 'loading') {
    return auth.isAuthenticated
      ? true
      : router.createUrlTree(['/auth/login']);
  }

  return toObservable(auth.authState).pipe(
    filter((state) => state !== 'loading'),
    take(1),
    map((state) =>
      state === 'authenticated'
        ? true
        : router.createUrlTree(['/auth/login']),
    ),
  );
};
