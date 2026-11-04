import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TenantScopedStateService {
  readonly invalidationVersion = signal(0);

  invalidate(): void {
    this.invalidationVersion.update((version) => version + 1);
  }
}
