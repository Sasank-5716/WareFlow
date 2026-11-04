import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading',
  template: `
    @if (visible()) {
      <div class="loading" role="status" aria-live="polite">
        <span class="spinner" aria-hidden="true"></span>
        <span>{{ label() }}</span>
      </div>
    }
  `,
  styles: `
    .loading { align-items: center; color: var(--color-text-muted); display: flex; gap: var(--space-3); padding: var(--space-6); }
    .spinner { animation: spin .8s linear infinite; border: 2px solid var(--color-border); border-top-color: var(--color-primary); border-radius: 50%; height: 1.25rem; width: 1.25rem; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `,
})
export class LoadingComponent {
  readonly visible = input(true);
  readonly label = input('Loading');
}
