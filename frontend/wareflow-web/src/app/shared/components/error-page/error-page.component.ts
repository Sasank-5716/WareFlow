import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../button/button.component';

@Component({
  imports: [RouterLink, ButtonComponent],
  selector: 'app-error-page',
  template: `
    <section class="error-page" aria-labelledby="error-title">
      <p class="eyebrow">WAREFLOW</p>
      <h1 id="error-title">{{ title() }}</h1>
      <p>{{ message() }}</p>
      <button appButton routerLink="/">Return home</button>
    </section>
  `,
  styles: `
    .error-page { margin: 10vh auto; max-width: 38rem; text-align: center; }
    .eyebrow { color: var(--color-primary); font-size: var(--font-size-sm); font-weight: 800; letter-spacing: .12em; }
    h1 { font-size: clamp(2rem, 5vw, 3.5rem); margin: var(--space-3) 0; }
    p:not(.eyebrow) { color: var(--color-text-muted); margin-bottom: var(--space-6); }
  `,
})
export class ErrorPageComponent {
  readonly title = input('Page not found');
  readonly message = input('The page you requested does not exist.');
}
