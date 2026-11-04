import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  template: `
    <section class="empty-state" aria-labelledby="empty-title">
      <div class="empty-icon" aria-hidden="true">○</div>
      <h2 id="empty-title">{{ title() }}</h2>
      <p>{{ message() }}</p>
      <ng-content />
    </section>
  `,
  styles: `
    .empty-state { color: var(--color-text-muted); padding: var(--space-12) var(--space-6); text-align: center; }
    .empty-icon { color: var(--color-primary); font-size: 2.5rem; }
    h2 { color: var(--color-text); font-size: var(--font-size-lg); margin: var(--space-3) 0 var(--space-2); }
    p { margin: 0 auto var(--space-5); max-width: 32rem; }
  `,
})
export class EmptyStateComponent {
  readonly title = input('Nothing here yet');
  readonly message = input('There is no data to display.');
}
