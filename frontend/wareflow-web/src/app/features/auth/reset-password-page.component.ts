import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { AuthService } from '../../core/auth/auth.service';
import { ApiError } from '../../core/models/api-error.model';

@Component({
  imports: [ReactiveFormsModule, RouterLink, ButtonComponent],
  template: `
    <section class="auth-card" aria-labelledby="reset-title">
      <h1 id="reset-title">Choose a new password</h1>
      @if (complete()) {
        <p class="status success">Your password has been updated.</p>
        <a appButton routerLink="/auth/login">Continue to sign in</a>
      } @else {
        @if (errorMessage()) { <p class="error" role="alert">{{ errorMessage() }}</p> }
        <form class="form" [formGroup]="form" (ngSubmit)="submit()">
          <label>Email <input type="email" formControlName="email" autocomplete="email" /></label>
          <label>Reset token <input formControlName="token" autocomplete="off" /></label>
          <label>New password <input type="password" formControlName="newPassword" autocomplete="new-password" /></label>
          <button appButton type="submit" [disabled]="form.invalid || submitting()">Update password</button>
        </form>
      }
      <div class="links"><a routerLink="/auth/login">Back to sign in</a></div>
    </section>
  `,
  styleUrl: './auth-page.scss',
})
export class ResetPasswordPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly submitting = signal(false);
  readonly complete = signal(false);
  readonly errorMessage = signal('');
  readonly form = inject(FormBuilder).nonNullable.group({
    email: [this.route.snapshot.queryParamMap.get('email') ?? '', [Validators.required, Validators.email]],
    token: [this.route.snapshot.queryParamMap.get('token') ?? '', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  submit(): void {
    if (this.form.invalid) return;
    this.submitting.set(true);
    this.auth.resetPassword(this.form.getRawValue()).subscribe({
      next: () => {
        this.submitting.set(false);
        this.complete.set(true);
      },
      error: (error: ApiError) => {
        this.submitting.set(false);
        this.errorMessage.set(error.message || 'This reset link is invalid or expired.');
      },
    });
  }
}
