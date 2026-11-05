import { Component, computed, effect, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TitleCasePipe } from '@angular/common';
import { filter } from 'rxjs';
import { ThemeMode, ThemeService } from '../services/theme.service';
import { NotificationService } from '../services/notification.service';
import { RouteProgressService } from '../services/route-progress.service';
import { AuthService } from '../auth/auth.service';
import { TenantSelectorComponent } from '../tenant/tenant-selector.component';
import { LoadingComponent } from '../../shared/components/loading/loading.component';
import { NavigationAccessService } from '../navigation/navigation-access.service';
import { NAVIGATION_REGISTRY } from '../navigation/navigation.registry';
import { TenantContextService } from '../tenant/tenant-context.service';

@Component({
  imports: [TitleCasePipe, RouterLink, RouterLinkActive, RouterOutlet, LoadingComponent, TenantSelectorComponent],
  selector: 'app-shell',
  template: `
    @if (auth.isAuthenticated) {
      <div class="shell-layout" [class.sidebar-open]="sidebarOpen()">
        <button class="scrim" type="button" aria-label="Close navigation" (click)="sidebarOpen.set(false)"></button>
        <aside class="sidebar" aria-label="Main navigation">
          <div class="sidebar-brand">
            <a class="brand" routerLink="/app" (click)="sidebarOpen.set(false)" aria-label="WAREFLOW overview">
              <span class="brand-mark" aria-hidden="true">W</span>
              <span>WAREFLOW</span>
            </a>
            <button class="icon-button mobile-only" type="button" aria-label="Close navigation" (click)="sidebarOpen.set(false)">×</button>
          </div>
          <div class="workspace-switcher">
            <span class="field-label">WORKSPACE</span>
            <app-tenant-selector />
          </div>
          <nav class="primary-nav">
            <span class="nav-label">Workspace</span>
            @for (item of navigationItems(); track item.id) {
              @if (canUse(item)) {
                <a class="nav-item" [routerLink]="item.route" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: item.id === 'overview' }" (click)="sidebarOpen.set(false)">
                  <span class="nav-icon" aria-hidden="true">{{ item.icon }}</span>
                  <span>{{ item.label }}</span>
                </a>
              } @else if (access.hasAccessData()) {
                <span class="nav-item disabled" [attr.title]="access.moduleReason(item.moduleKey) ?? 'Not included in this workspace'">
                  <span class="nav-icon" aria-hidden="true">{{ item.icon }}</span>
                  <span>{{ item.label }}</span>
                  <span class="lock" aria-label="Not available">·</span>
                </span>
              }
            }
          </nav>
          <div class="sidebar-footer">
            <span class="status-dot" aria-hidden="true"></span>
            <span>Workspace connected</span>
          </div>
        </aside>

        <div class="shell-content">
          <header class="topbar">
            <button class="icon-button mobile-only" type="button" aria-label="Open navigation" (click)="sidebarOpen.set(true)">☰</button>
            <div class="breadcrumbs" aria-label="Breadcrumbs">
              <a routerLink="/app">Workspace</a><span aria-hidden="true">/</span><strong>{{ currentRouteLabel() }}</strong>
            </div>
            <div class="topbar-actions">
              <label class="warehouse-context">
                <span class="sr-only">Warehouse context</span>
                <select disabled title="Warehouse context will be available when warehouse setup is complete">
                  <option>All warehouses</option>
                </select>
              </label>
              <button class="topbar-button search-button" type="button" (click)="showComingSoon('Global search')">
                <span aria-hidden="true">⌕</span><span class="search-label">Search</span><kbd>⌘ K</kbd>
              </button>
              <button class="topbar-button" type="button" (click)="showComingSoon('Notifications')" aria-label="Notifications, coming soon">♢</button>
              <button class="topbar-button ai-button" type="button" (click)="showComingSoon('AI assistant')"><span aria-hidden="true">✦</span><span class="ai-label">Ask WAREFLOW</span></button>
              <label class="theme-control">
                <span class="sr-only">Theme</span>
                <select [value]="theme.mode()" (change)="changeTheme($event)" aria-label="Theme">
                  @for (option of themeOptions; track option) { <option [value]="option">{{ option | titlecase }}</option> }
                </select>
              </label>
              <button class="profile-button" type="button" aria-label="Open user menu" (click)="profileOpen.set(!profileOpen())">
                <span class="avatar">{{ initials() }}</span>
                <span class="profile-copy"><strong>{{ auth.user()?.displayName ?? 'User' }}</strong><small>Account</small></span>
                <span aria-hidden="true">⌄</span>
              </button>
              @if (profileOpen()) {
                <div class="profile-menu">
                  <button type="button" (click)="logout()">Sign out</button>
                </div>
              }
            </div>
          </header>
          <main class="shell-main">
            @if (progress.loading()) { <app-loading [visible]="true" label="Loading workspace" /> }
            <router-outlet />
          </main>
        </div>
      </div>
    } @else {
      <div class="public-shell">
        <a class="brand public-brand" routerLink="/" aria-label="WAREFLOW home"><span class="brand-mark" aria-hidden="true">W</span><span>WAREFLOW</span></a>
        <router-outlet />
      </div>
    }
    <div class="toast-region" aria-live="polite">
      @for (notification of notifications.notifications(); track notification.id) {
        <div class="toast" [attr.data-tone]="notification.tone" role="status">
          {{ notification.message }}
          <button type="button" class="icon-button" (click)="notifications.dismiss(notification.id)"><span class="sr-only">Dismiss</span>×</button>
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
  readonly access = inject(NavigationAccessService);
  readonly tenant = inject(TenantContextService);
  private readonly router = inject(Router);
  readonly themeOptions: readonly ThemeMode[] = ['light', 'dark', 'system'];
  readonly sidebarOpen = signal(false);
  readonly profileOpen = signal(false);
  readonly currentRouteLabel = signal('Overview');
  readonly navigationItems = computed(() => NAVIGATION_REGISTRY.filter((item) =>
    (!item.moduleKey && !item.permission) ||
    (this.access.hasAccessData() && (!item.moduleKey ||
      this.access.effectiveModules().some((module) => module.moduleKey === item.moduleKey))),
  ));
  readonly initials = computed(() => (this.auth.user()?.displayName ?? this.auth.user()?.email ?? 'U')
    .split(/\s+/).map((part) => part[0]).join('').slice(0, 2).toUpperCase());

  constructor() {
    this.router.events.pipe(filter((event) => event instanceof NavigationEnd)).subscribe((event) => {
      const route = NAVIGATION_REGISTRY.find((item) => item.route === (event as NavigationEnd).urlAfterRedirects);
      this.currentRouteLabel.set(route?.label ?? 'Workspace');
    });
    effect(() => {
      if (this.auth.isAuthenticated && this.access.accessState() === 'idle') {
        this.tenant.activeTenant();
        this.access.load();
      }
    });
  }

  canUse(item: typeof NAVIGATION_REGISTRY[number]): boolean {
    return this.access.canUse(item.moduleKey, item.permission);
  }

  changeTheme(event: Event): void {
    this.theme.setMode((event.target as HTMLSelectElement).value as ThemeMode);
  }

  showComingSoon(feature: string): void {
    this.notifications.show({ tone: 'info', message: `${feature} will be connected in a later phase.` });
  }

  logout(): void {
    this.profileOpen.set(false);
    this.auth.logout().subscribe({
      next: () => void this.router.navigate(['/auth/login']),
      error: () => this.notifications.show({ tone: 'error', message: 'Sign out failed. Please try again.' }),
    });
  }
}
