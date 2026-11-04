import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class PermissionService {
  private readonly permissions = signal<ReadonlySet<string>>(new Set());

  setPermissions(permissions: readonly string[]): void {
    this.permissions.set(new Set(permissions));
  }

  has(permission: string): boolean {
    return this.permissions().has(permission);
  }
}
