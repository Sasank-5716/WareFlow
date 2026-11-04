import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-dialog',
  template: `
    @if (open()) {
      <div class="backdrop" role="presentation" (click)="closed.emit()">
        <section
          class="dialog"
          role="dialog"
          aria-modal="true"
          [attr.aria-labelledby]="titleId"
          (click)="$event.stopPropagation()"
        >
          <header><h2 [id]="titleId">{{ title() }}</h2><button type="button" class="close" (click)="closed.emit()" aria-label="Close dialog">&times;</button></header>
          <ng-content />
        </section>
      </div>
    }
  `,
  styles: `
    .backdrop { align-items: center; background: rgb(0 0 0 / 45%); display: flex; inset: 0; justify-content: center; padding: var(--space-5); position: fixed; z-index: 20; }
    .dialog { background: var(--color-surface-elevated); border: 1px solid var(--color-border); border-radius: var(--radius-lg); box-shadow: var(--shadow-lg); max-width: 32rem; padding: var(--space-6); width: 100%; }
    header { align-items: center; display: flex; justify-content: space-between; } h2 { font-size: var(--font-size-lg); margin: 0; } .close { background: none; border: 0; color: var(--color-text-muted); cursor: pointer; font-size: 1.5rem; }
  `,
})
export class DialogComponent {
  readonly open = input(false);
  readonly title = input('Dialog');
  readonly closed = output<void>();
  protected readonly titleId = 'wareflow-dialog-title';
}
