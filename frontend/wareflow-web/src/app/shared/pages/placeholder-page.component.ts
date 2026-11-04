import { Component, input } from '@angular/core';
import { CardComponent } from '../components/card/card.component';
import { EmptyStateComponent } from '../components/empty-state/empty-state.component';

@Component({
  imports: [CardComponent, EmptyStateComponent],
  selector: 'app-placeholder-page',
  template: `
    <p class="eyebrow">FOUNDATION</p>
    <h1>{{ title() }}</h1>
    <p class="lede">{{ description() }}</p>
    <app-card>
      <app-empty-state
        title="Feature coming soon"
        message="This route is reserved for the {{ title() }} experience. Business functionality will be added in its feature boundary."
      />
    </app-card>
  `,
  styles: `
    :host { display: block; }
    .eyebrow { color: var(--color-primary); font-size: var(--font-size-sm); font-weight: 800; letter-spacing: .12em; }
    h1 { font-size: clamp(2rem, 5vw, 3.5rem); margin: var(--space-2) 0; }
    .lede { color: var(--color-text-muted); margin: 0 0 var(--space-8); max-width: 42rem; }
  `,
})
export class PlaceholderPageComponent {
  readonly title = input.required<string>();
  readonly description = input.required<string>();
}
