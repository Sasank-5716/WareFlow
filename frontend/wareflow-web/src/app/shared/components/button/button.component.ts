import { Component, input } from '@angular/core';

@Component({
  selector: '[appButton]',
  template: `<ng-content />`,
  host: {
    '[attr.data-variant]': 'variant()',
    '[attr.data-size]': 'size()',
    '[attr.disabled]': 'disabled() ? "" : null',
    '[class.button]': 'true',
  },
  styles: `
    :host { align-items: center; border: 1px solid transparent; border-radius: var(--radius-sm); cursor: pointer; display: inline-flex; font: inherit; font-weight: 650; justify-content: center; min-height: 2.5rem; padding: 0 var(--space-4); transition: background-color var(--motion-fast), transform var(--motion-fast); }
    :host(:focus-visible) { outline: 3px solid var(--color-focus); outline-offset: 2px; }
    :host(:active:not(:disabled)) { transform: translateY(1px); }
    :host([data-variant="primary"]) { background: var(--color-primary); color: var(--color-on-primary); }
    :host([data-variant="secondary"]) { background: var(--color-surface); border-color: var(--color-border); color: var(--color-text); }
    :host([data-variant="ghost"]) { background: transparent; color: var(--color-primary); }
    :host([data-size="sm"]) { min-height: 2rem; padding: 0 var(--space-3); }
    :host(:disabled) { cursor: not-allowed; opacity: .55; }
  `,
})
export class ButtonComponent {
  readonly variant = input<'primary' | 'secondary' | 'ghost'>('primary');
  readonly size = input<'sm' | 'md'>('md');
  readonly disabled = input(false);
}
