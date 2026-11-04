import { DOCUMENT } from '@angular/common';
import { Injectable, inject, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark' | 'system';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  readonly mode = signal<ThemeMode>('system');

  constructor() {
    this.apply(this.mode());
  }

  setMode(mode: ThemeMode): void {
    this.mode.set(mode);
    this.apply(mode);
  }

  private apply(mode: ThemeMode): void {
    const root = this.document.documentElement;
    root.dataset['theme'] = mode;
    root.style.colorScheme = mode === 'system' ? 'light dark' : mode;
  }
}
