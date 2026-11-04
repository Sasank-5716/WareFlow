import { Component, inject, signal } from '@angular/core';
import { TenantContextService } from './tenant-context.service';
import { NotificationService } from '../services/notification.service';

@Component({
  selector: 'app-tenant-selector',
  template: `
    @if (context.tenantState() === 'ready') {
      <label class="tenant-picker">
        <span class="sr-only">Company</span>
        <select
          [value]="context.activeTenant()?.id ?? ''"
          [disabled]="switching()"
          (change)="switchTenant($event)"
        >
          <option value="" disabled>Select company</option>
          @for (tenant of context.tenants(); track tenant.id) {
            <option [value]="tenant.id">{{ tenant.name }}</option>
          }
        </select>
      </label>
    } @else if (context.tenantState() === 'empty') {
      <span class="tenant-empty">No companies available</span>
    } @else if (context.tenantState() === 'error') {
      <span class="tenant-error">Companies unavailable</span>
    }
  `,
  styles: `
    .tenant-picker select { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-sm); color: var(--color-text); max-width: 13rem; padding: .4rem .6rem; }
    .tenant-empty, .tenant-error { color: var(--color-text-muted); font-size: var(--font-size-sm); }
    .tenant-error { color: var(--color-danger); }
  `,
})
export class TenantSelectorComponent {
  readonly context = inject(TenantContextService);
  private readonly notifications = inject(NotificationService);
  readonly switching = signal(false);

  switchTenant(event: Event): void {
    const tenantId = (event.target as HTMLSelectElement).value;
    if (!tenantId || tenantId === this.context.activeTenant()?.id) {
      return;
    }

    this.switching.set(true);
    this.context.selectTenant(tenantId).subscribe({
      next: () => this.switching.set(false),
      error: () => {
        this.switching.set(false);
        this.notifications.show({
          tone: 'error',
          message: 'We could not switch companies. Your current company is unchanged.',
        });
      },
    });
  }
}
