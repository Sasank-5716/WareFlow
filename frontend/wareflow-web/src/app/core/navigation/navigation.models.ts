export interface NavigationItem {
  readonly id: string;
  readonly label: string;
  readonly route: string;
  readonly moduleKey?: string;
  readonly permission?: string;
  readonly description: string;
  readonly icon: string;
}

export interface EffectiveModule {
  readonly moduleKey: string;
  readonly displayName: string;
  readonly effectiveAccess: boolean;
  readonly reason: string | null;
}
