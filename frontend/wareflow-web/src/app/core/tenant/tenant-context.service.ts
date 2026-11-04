import { Injectable, signal } from '@angular/core';

export interface TenantContext {
  readonly id: string;
  readonly name: string;
}

@Injectable({ providedIn: 'root' })
export class TenantContextService {
  readonly activeTenant = signal<TenantContext | null>(null);

  selectTenant(tenant: TenantContext): void {
    this.activeTenant.set(tenant);
  }

  clear(): void {
    this.activeTenant.set(null);
  }
}
