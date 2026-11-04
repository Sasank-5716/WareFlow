import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  imports: [ReactiveFormsModule, RouterLink, ButtonComponent],
  template: `
    <section class="auth-card" aria-labelledby="verify-title">
      <h1 id="verify-title">Verify your email</h1>
      @if (verified()) {
        <p class="status success">Your email is verified. You can sign in now.</p>
        <a appButton routerLink="/auth/login">Continue to sign in</a>
      } @else {
        <p class="lede">Use the verification link from your email, or enter its values below.</p>
        @if (errorMessage()) { <p class="error" role="alert">{{ errorMessage() }}</p> }
        <form class="form" [formGroup]="form" (ngSubmit)="submit()">
          <label>User ID <input formControlName="userId" autocomplete="off" /></label>
          <label>Verification token <input formControlName="token" autocomplete="off" /></label>
          <button appButton type="submit" [disabled]="form.invalid || submitting()">Verify email</button>
        </form>
      }
    </section>
  `,
  styleUrl: './auth-page.scss',
})
export class VerifyEmailPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly submitting = signal(false);
  readonly verified = signal(false);
  readonly errorMessage = signal('');
  readonly form = inject(FormBuilder).nonNullable.group({
    userId: [this.route.snapshot.queryParamMap.get('userId') ?? '', Validators.required],
    token: [this.route.snapshot.queryParamMap.get('token') ?? '', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) return;
    this.submitting.set(true);
    const { userId, token } = this.form.getRawValue();
    this.auth.confirmEmail(userId, token).subscribe({
      next: () => {
        this.submitting.set(false);
        this.verified.set(true);
      },
      error: () => {
        this.submitting.set(false);
        this.errorMessage.set('This verification link is invalid or expired.');
      },
    });
  }
}
