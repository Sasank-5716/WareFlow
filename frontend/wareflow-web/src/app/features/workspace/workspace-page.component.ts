import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { CardComponent } from '../../shared/components/card/card.component';
import { BadgeComponent } from '../../shared/components/badge/badge.component';

@Component({
  imports: [RouterLink, ButtonComponent, CardComponent, BadgeComponent],
  selector: 'app-workspace-page',
  template: `
    <section class="page-heading">
      <div>
        <p class="eyebrow">WORKSPACE OVERVIEW</p>
        <h1>Good morning, {{ greetingName }}</h1>
        <p class="lede">Your operational workspace is ready when you are.</p>
      </div>
      <app-badge tone="success">Connected</app-badge>
    </section>
    <section class="empty-state" aria-labelledby="empty-title">
      <div class="empty-icon" aria-hidden="true">⌁</div>
      <p class="eyebrow">A CLEAR START</p>
      <h2 id="empty-title">Your operational view is taking shape</h2>
      <p>Once your workspace has activity, inventory, orders, and performance signals will appear here. For now, choose a workspace area to get started.</p>
      <div class="quick-actions">
        <a appButton variant="primary" routerLink="/app/inventory">Open inventory</a>
        <a appButton variant="secondary" routerLink="/app/settings">Configure workspace</a>
      </div>
    </section>
    <section class="next-grid" aria-label="Workspace setup">
      @for (item of setupItems; track item.title) {
        <app-card>
          <span class="step">{{ item.step }}</span>
          <h2>{{ item.title }}</h2>
          <p>{{ item.description }}</p>
        </app-card>
      }
    </section>
  `,
  styles: `
    .page-heading { align-items: start; display: flex; justify-content: space-between; margin-bottom: 2rem; }
    .eyebrow { color: var(--color-primary); font-size: .72rem; font-weight: 800; letter-spacing: .13em; margin: 0 0 .55rem; }
    h1 { font-size: clamp(1.8rem, 4vw, 2.65rem); letter-spacing: -.03em; margin: 0; }
    .lede { color: var(--color-text-muted); margin: .55rem 0 0; }
    .empty-state { background: var(--color-surface-elevated); border: 1px dashed var(--color-border-strong); border-radius: var(--radius-lg); margin-bottom: 1.25rem; padding: clamp(2rem, 5vw, 4.5rem) 1.5rem; text-align: center; }
    .empty-icon { align-items: center; background: color-mix(in srgb, var(--color-primary) 12%, transparent); border-radius: 50%; color: var(--color-primary); display: flex; font-size: 2.5rem; height: 4.5rem; justify-content: center; margin: 0 auto 1.25rem; width: 4.5rem; }
    .empty-state h2 { font-size: clamp(1.35rem, 3vw, 1.8rem); margin: 0; }.empty-state > p:not(.eyebrow) { color: var(--color-text-muted); line-height: 1.6; margin: .75rem auto 1.5rem; max-width: 34rem; }
    .quick-actions { display: flex; flex-wrap: wrap; gap: .75rem; justify-content: center; }.quick-actions a { text-decoration: none; }
    .next-grid { display: grid; gap: 1rem; grid-template-columns: repeat(3, 1fr); }.next-grid app-card { min-height: 8rem; }.next-grid h2 { font-size: 1rem; margin: .65rem 0 .35rem; }.next-grid p { color: var(--color-text-muted); font-size: .88rem; line-height: 1.5; margin: 0; }.step { color: var(--color-primary); font-size: .75rem; font-weight: 800; }
    @media (max-width: 45rem) { .page-heading { display: block; }.page-heading app-badge { display: inline-block; margin-top: 1rem; }.next-grid { grid-template-columns: 1fr; } }
  `,
})
export class WorkspacePageComponent {
  readonly greetingName = 'there';
  readonly setupItems = [
    { step: '01', title: 'Connect your operations', description: 'Set up warehouses, products, and the teams who keep them moving.' },
    { step: '02', title: 'Build your first workflow', description: 'Use the feature areas to establish a consistent operating rhythm.' },
    { step: '03', title: 'See the signal', description: 'Your real operational metrics will appear here as work happens.' },
  ];
}
