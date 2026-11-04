import { Injectable, signal } from '@angular/core';

export type FeatureKey = 'inventory' | 'orders' | 'settings';

@Injectable({ providedIn: 'root' })
export class FeatureAccessService {
  private readonly enabled = signal<ReadonlySet<FeatureKey>>(new Set());

  setEnabled(features: readonly FeatureKey[]): void {
    this.enabled.set(new Set(features));
  }

  isEnabled(feature: FeatureKey): boolean {
    return this.enabled().has(feature);
  }
}
