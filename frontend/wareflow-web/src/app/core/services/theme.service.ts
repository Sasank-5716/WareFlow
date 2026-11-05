import { DOCUMENT } from '@angular/common';
import { Injectable, inject, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark' | 'system';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  readonly mode = signal<ThemeMode>(this.readMode());

  constructor() {
    this.apply(this.mode());
  }

  setMode(mode: ThemeMode): void {
    this.mode.set(mode);
    localStorage.setItem('wareflow-theme', mode);
    this.apply(mode);
  }

  private readMode(): ThemeMode {
    const saved = localStorage.getItem('wareflow-theme');
    return saved === 'light' || saved === 'dark' || saved === 'system' ? saved : 'system';
  }

  private apply(mode: ThemeMode): void {
    const root = this.document.documentElement;
    root.dataset['theme'] = mode;
    root.style.colorScheme = mode === 'system' ? 'light dark' : mode;
  }
}
