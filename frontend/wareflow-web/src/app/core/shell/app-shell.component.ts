import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { ThemeMode, ThemeService } from '../services/theme.service';
import { NotificationService } from '../services/notification.service';
import { RouteProgressService } from '../services/route-progress.service';
import { AuthService } from '../auth/auth.service';
import { TenantSelectorComponent } from '../tenant/tenant-selector.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { LoadingComponent } from '../../shared/components/loading/loading.component';

@Component({
  imports: [
    RouterLink,
    RouterOutlet,
    LoadingComponent,
    TenantSelectorComponent,
    ButtonComponent,
  ],
  selector: 'app-shell',
  template: `
    <header class="shell-header">
      <a class="brand" routerLink="/" aria-label="WAREFLOW home">
        <span class="brand-mark" aria-hidden="true">W</span>
        <span>WAREFLOW</span>
      </a>
      <nav aria-label="Primary">
        <a routerLink="/app" routerLinkActive="active">Workspace</a>
        <a routerLink="/about" routerLinkActive="active">About</a>
      </nav>
      @if (auth.isAuthenticated) {
        <app-tenant-selector />
        <button appButton variant="ghost" size="sm" type="button" (click)="logout()">
          Sign out
        </button>
      } @else {
        <a appButton variant="secondary" size="sm" routerLink="/auth/login">Sign in</a>
      }
      <label class="theme-select">
        <span class="sr-only">Theme</span>
        <select [value]="theme.mode()" (change)="changeTheme($event)">
          @for (option of themeOptions; track option) {
            <option [value]="option">{{ option }}</option>
          }
        </select>
      </label>
    </header>
    <main class="shell-main">
      <router-outlet />
    </main>
    <app-loading [visible]="progress.loading()" label="Loading page" />
    <div class="toast-region" aria-live="polite">
      @for (notification of notifications.notifications(); track notification.id) {
        <div class="toast" [attr.data-tone]="notification.tone" role="status">
          {{ notification.message }}
          <button type="button" class="icon-button" (click)="notifications.dismiss(notification.id)">
            <span class="sr-only">Dismiss</span>&times;
          </button>
        </div>
      }
    </div>
  `,
  styleUrl: './app-shell.component.scss',
})
export class AppShellComponent {
  readonly theme = inject(ThemeService);
  readonly notifications = inject(NotificationService);
  readonly progress = inject(RouteProgressService);
  readonly auth = inject(AuthService);
  readonly themeOptions: readonly ThemeMode[] = ['light', 'dark', 'system'];
  changeTheme(event: Event): void {
    this.theme.setMode((event.target as HTMLSelectElement).value as ThemeMode);
  }

  logout(): void {
    this.auth.logout().subscribe({
      error: () =>
        this.notifications.show({
          tone: 'error',
          message: 'Sign out failed. Please try again.',
        }),
    });
  }
}
