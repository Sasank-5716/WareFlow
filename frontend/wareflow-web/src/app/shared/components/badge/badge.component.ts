import { Component, input } from '@angular/core';

@Component({
  selector: 'app-badge',
  template: `<span [attr.data-tone]="tone()"><ng-content /></span>`,
  styles: `
    :host span { background: var(--color-status-bg); border-radius: var(--radius-full); color: var(--color-status); display: inline-block; font-size: .75rem; font-weight: 700; padding: .2rem .55rem; }
    :host span[data-tone="success"] { --color-status: var(--color-success); --color-status-bg: var(--color-success-subtle); }
    :host span[data-tone="warning"] { --color-status: var(--color-warning); --color-status-bg: var(--color-warning-subtle); }
    :host span[data-tone="danger"] { --color-status: var(--color-danger); --color-status-bg: var(--color-danger-subtle); }
  `,
})
export class BadgeComponent {
  readonly tone = input<'neutral' | 'success' | 'warning' | 'danger'>('neutral');
}
