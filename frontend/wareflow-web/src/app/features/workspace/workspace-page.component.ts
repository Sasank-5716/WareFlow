import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { CardComponent } from '../../shared/components/card/card.component';
import { BadgeComponent } from '../../shared/components/badge/badge.component';

@Component({
  imports: [RouterLink, ButtonComponent, CardComponent, BadgeComponent],
  selector: 'app-workspace-page',
  template: `
    <section class="welcome">
      <div>
        <p class="eyebrow">OPERATIONS</p>
        <h1>Workspace foundation</h1>
        <p class="lede">Your authenticated operational app will grow here, feature by feature.</p>
      </div>
      <app-badge tone="success">Foundation ready</app-badge>
    </section>
    <div class="feature-grid">
      @for (feature of features; track feature.path) {
        <app-card>
          <h2>{{ feature.name }}</h2>
          <p>{{ feature.description }}</p>
          <a appButton variant="secondary" [routerLink]="feature.path">Open {{ feature.name }}</a>
        </app-card>
      }
    </div>
  `,
  styles: `
    .welcome { align-items: start; display: flex; gap: var(--space-5); justify-content: space-between; margin-bottom: var(--space-8); }
    .eyebrow { color: var(--color-primary); font-size: var(--font-size-sm); font-weight: 800; letter-spacing: .12em; }
    h1 { font-size: clamp(2rem, 5vw, 3.5rem); margin: var(--space-2) 0; }
    .lede, app-card p { color: var(--color-text-muted); }
    .feature-grid { display: grid; gap: var(--space-5); grid-template-columns: repeat(auto-fit, minmax(15rem, 1fr)); }
    app-card { display: flex; flex-direction: column; gap: var(--space-3); }
    app-card h2 { margin: 0; }
    app-card p { flex: 1; margin: 0; }
    a { text-decoration: none; }
    @media (max-width: 40rem) { .welcome { display: block; } app-badge { display: inline-block; margin-top: var(--space-3); } }
  `,
})
export class WorkspacePageComponent {
  readonly features = [
    { name: 'Inventory', path: 'inventory', description: 'Stock, locations, and movement visibility.' },
    { name: 'Orders', path: 'orders', description: 'A future home for fulfillment workflows.' },
    { name: 'Settings', path: 'settings', description: 'Tenant and workspace configuration.' },
  ];
}
