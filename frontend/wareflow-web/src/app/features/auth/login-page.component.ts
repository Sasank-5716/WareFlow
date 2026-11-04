import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { AuthService } from '../../core/auth/auth.service';
import { ApiError } from '../../core/models/api-error.model';

@Component({
  imports: [ReactiveFormsModule, RouterLink, ButtonComponent],
  template: `
    <section class="auth-card" aria-labelledby="login-title">
      <h1 id="login-title">Sign in</h1>
      <p class="lede">Access your WAREFLOW workspace.</p>
      @if (errorMessage()) { <p class="error" role="alert">{{ errorMessage() }}</p> }
      <form class="form" [formGroup]="form" (ngSubmit)="submit()">
        <label>Email <input type="email" formControlName="email" autocomplete="email" /></label>
        <label>Password <input type="password" formControlName="password" autocomplete="current-password" /></label>
        <label><span><input type="checkbox" formControlName="rememberMe" /> Remember me</span></label>
        <button appButton type="submit" [disabled]="form.invalid || submitting()">Sign in</button>
      </form>
      <div class="links">
        <a routerLink="/auth/forgot-password">Forgot password?</a>
        <a routerLink="/auth/register">Create an account</a>
      </div>
    </section>
  `,
  styleUrl: './auth-page.scss',
})
export class LoginPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly submitting = signal(false);
  readonly errorMessage = signal('');
  readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    rememberMe: false,
  });

  submit(): void {
    if (this.form.invalid) return;
    this.submitting.set(true);
    this.errorMessage.set('');
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigateByUrl('/app'),
      error: (error: ApiError) => {
        this.submitting.set(false);
        this.errorMessage.set(
          error.status === 423
            ? 'This account is temporarily locked.'
            : 'Email or password is incorrect.',
        );
      },
    });
  }
}
