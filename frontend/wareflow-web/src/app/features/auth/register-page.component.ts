import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { AuthService } from '../../core/auth/auth.service';
import { ApiError } from '../../core/models/api-error.model';

@Component({
  imports: [ReactiveFormsModule, RouterLink, ButtonComponent],
  template: `
    <section class="auth-card" aria-labelledby="register-title">
      <h1 id="register-title">Create your account</h1>
      <p class="lede">Start your WAREFLOW workspace.</p>
      @if (errorMessage()) { <p class="error" role="alert">{{ errorMessage() }}</p> }
      <form class="form" [formGroup]="form" (ngSubmit)="submit()">
        <label>Display name <input formControlName="displayName" autocomplete="name" /></label>
        <label>Email <input type="email" formControlName="email" autocomplete="email" /></label>
        <label>Password <input type="password" formControlName="password" autocomplete="new-password" /></label>
        <button appButton type="submit" [disabled]="form.invalid || submitting()">Create account</button>
      </form>
      <div class="links"><a routerLink="/auth/login">Already have an account? Sign in</a></div>
    </section>
  `,
  styleUrl: './auth-page.scss',
})
export class RegisterPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly submitting = signal(false);
  readonly errorMessage = signal('');
  readonly form = inject(FormBuilder).nonNullable.group({
    displayName: '',
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  submit(): void {
    if (this.form.invalid) return;
    this.submitting.set(true);
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () =>
        this.router.navigate(['/auth/verify-email'], {
          queryParams: { email: this.form.controls.email.value },
        }),
      error: (error: ApiError) => {
        this.submitting.set(false);
        this.errorMessage.set(error.message || 'Registration failed.');
      },
    });
  }
}
