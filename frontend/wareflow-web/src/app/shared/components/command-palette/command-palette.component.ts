import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-command-palette',
  template: `
    @if (open()) {
      <div class="backdrop" (click)="closed.emit()">
        <section class="palette" role="dialog" aria-label="Command palette" (click)="$event.stopPropagation()">
          <input type="search" placeholder="Search commands" aria-label="Search commands" />
          <ng-content />
        </section>
      </div>
    }
  `,
  styles: `
    .backdrop { background: rgb(0 0 0 / 45%); inset: 0; padding: 12vh 1rem; position: fixed; z-index: 25; } .palette { background: var(--color-surface-elevated); border: 1px solid var(--color-border); border-radius: var(--radius-lg); box-shadow: var(--shadow-lg); margin: auto; max-width: 40rem; padding: var(--space-3); } input { border: 0; border-bottom: 1px solid var(--color-border); color: var(--color-text); background: transparent; padding: var(--space-3); width: 100%; } input:focus-visible { outline: 3px solid var(--color-focus); }
  `,
})
export class CommandPaletteComponent {
  readonly open = input(false);
  readonly closed = output<void>();
}
