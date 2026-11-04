import { Component } from '@angular/core';

@Component({
  selector: 'app-card',
  template: `<ng-content />`,
  styles: `
    :host { background: var(--color-surface-elevated); border: 1px solid var(--color-border); border-radius: var(--radius-lg); box-shadow: var(--shadow-sm); display: block; padding: var(--space-6); }
  `,
})
export class CardComponent {}
