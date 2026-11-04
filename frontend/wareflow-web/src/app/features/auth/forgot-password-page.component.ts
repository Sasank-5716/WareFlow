import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  imports: [ReactiveFormsModule, RouterLink, ButtonComponent],
  template: `
    <section class="auth-card" aria-labelledby="forgot-title">
      <h1 id="forgot-title">Reset your password</h1>
      @if (sent()) {
        <p class="status success">If an account exists for that email, reset instructions are on their way.</p>
      } @else {
        <p class="lede">Enter your email and we will send reset instructions.</p>
        @if (errorMessage()) { <p class="error" role="alert">{{ errorMessage() }}</p> }
        <form class="form" [formGroup]="form" (ngSubmit)="submit()">
          <label>Email <input type="email" formControlName="email" autocomplete="email" /></label>
          <button appButton type="submit" [disabled]="form.invalid || submitting()">Send reset email</button>
        </form>
      }
      <div class="links"><a routerLink="/auth/login">Back to sign in</a></div>
    </section>
  `,
  styleUrl: './auth-page.scss',
})
export class ForgotPasswordPageComponent {
  private readonly auth = inject(AuthService);
  readonly submitting = signal(false);
  readonly sent = signal(false);
  readonly errorMessage = signal('');
  readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  submit(): void {
    if (this.form.invalid) return;
    this.submitting.set(true);
    this.auth.requestPasswordReset(this.form.controls.email.value).subscribe({
      next: () => {
        this.submitting.set(false);
        this.sent.set(true);
      },
      error: () => {
        this.submitting.set(false);
        this.errorMessage.set('We could not request a reset email. Please try again.');
      },
    });
  }
}
