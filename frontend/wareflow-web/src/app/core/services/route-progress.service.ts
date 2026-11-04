import { Injectable, signal } from '@angular/core';
import {
  Event,
  NavigationCancel,
  NavigationEnd,
  NavigationError,
  NavigationStart,
  Router,
} from '@angular/router';

@Injectable({ providedIn: 'root' })
export class RouteProgressService {
  readonly loading = signal(false);

  constructor(router: Router) {
    router.events.subscribe((event: Event) => {
      if (event instanceof NavigationStart) this.loading.set(true);
      if (
        event instanceof NavigationEnd ||
        event instanceof NavigationCancel ||
        event instanceof NavigationError
      ) {
        this.loading.set(false);
      }
    });
  }
}
