import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  imports: [RouterLink, RouterOutlet],
  template: `
    <main class="auth-layout">
      <a class="brand" routerLink="/">WAREFLOW</a>
      <router-outlet />
    </main>
  `,
  styles: `
    .auth-layout { margin: 0 auto; max-width: 32rem; padding: 8vh var(--space-5); }
    .brand { color: var(--color-text); font-weight: 800; letter-spacing: .08em; text-decoration: none; }
  `,
})
export class AuthShellComponent {}
