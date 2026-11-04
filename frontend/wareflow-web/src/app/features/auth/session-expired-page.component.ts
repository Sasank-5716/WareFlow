import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../shared/components/button/button.component';

@Component({
  imports: [RouterLink, ButtonComponent],
  template: `
    <section class="auth-card" aria-labelledby="expired-title">
      <h1 id="expired-title">Your session expired</h1>
      <p class="lede">Sign in again to continue. Unsaved work was not submitted.</p>
      <a appButton routerLink="/auth/login">Sign in again</a>
    </section>
  `,
  styleUrl: './auth-page.scss',
})
export class SessionExpiredPageComponent {}
