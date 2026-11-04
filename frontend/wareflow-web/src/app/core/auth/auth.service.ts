import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import {
  catchError,
  firstValueFrom,
  map,
  Observable,
  of,
  switchMap,
  tap,
  throwError,
} from 'rxjs';
import { ApiError } from '../models/api-error.model';
import { TenantContextService } from '../tenant/tenant-context.service';

export interface AuthenticatedUser {
  readonly id: string;
  readonly email: string;
  readonly displayName: string | null;
  readonly isEmailConfirmed: boolean;
}

export interface RegisterRequest {
  readonly email: string;
  readonly password: string;
  readonly displayName: string | null;
}

export interface LoginRequest {
  readonly email: string;
  readonly password: string;
  readonly rememberMe: boolean;
}

export interface ResetPasswordRequest {
  readonly email: string;
  readonly token: string;
  readonly newPassword: string;
}

export type AuthState = 'loading' | 'anonymous' | 'authenticated';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly state = signal<AuthState>('loading');
  private readonly userState = signal<AuthenticatedUser | null>(null);

  readonly authState = this.state.asReadonly();
  readonly user = this.userState.asReadonly();

  constructor(
    private readonly http: HttpClient,
    private readonly tenantContext: TenantContextService,
  ) {}

  async bootstrap(): Promise<void> {
    try {
      await firstValueFrom(this.http.get<void>('/api/auth/csrf'));
      const user = await firstValueFrom(
        this.http.get<AuthenticatedUser>('/api/auth/me').pipe(
          catchError((error: ApiError) =>
            error.status === 401 ? of(null) : throwError(() => error),
          ),
        ),
      );

      if (!user) {
        this.setAnonymous();
        return;
      }

      this.userState.set(user);
      this.state.set('authenticated');
      await firstValueFrom(this.tenantContext.bootstrap());
      await this.tenantContext.restoreActiveTenant();
    } catch (error) {
      console.error('Session bootstrap failed', error);
      this.setAnonymous();
    }
  }

  login(request: LoginRequest): Observable<void> {
    return this.withCsrf(() =>
      this.http.post<void>('/api/auth/login', request).pipe(
        switchMap(() => this.restoreSession()),
        map(() => undefined),
      ),
    );
  }

  register(request: RegisterRequest): Observable<void> {
    return this.withCsrf(() =>
      this.http.post<void>('/api/auth/register', request),
    );
  }

  confirmEmail(userId: string, token: string): Observable<void> {
    return this.withCsrf(() =>
      this.http.post<void>('/api/auth/confirm-email', { userId, token }),
    );
  }

  requestPasswordReset(email: string): Observable<void> {
    return this.withCsrf(() =>
      this.http.post<void>('/api/auth/forgot-password', { email }),
    );
  }

  resetPassword(request: ResetPasswordRequest): Observable<void> {
    return this.withCsrf(() =>
      this.http.post<void>('/api/auth/reset-password', request),
    );
  }

  logout(): Observable<void> {
    return this.withCsrf(() =>
      this.http.post<void>('/api/auth/logout', null).pipe(
        tap(() => {
          this.setAnonymous();
          this.tenantContext.clear();
        }),
      ),
    );
  }

  markSessionExpired(): void {
    this.setAnonymous();
    this.tenantContext.clear();
  }

  get isAuthenticated(): boolean {
    return this.authState() === 'authenticated';
  }

  private restoreSession(): Observable<AuthenticatedUser> {
    return this.http.get<AuthenticatedUser>('/api/auth/me').pipe(
      tap((user) => {
        this.userState.set(user);
        this.state.set('authenticated');
      }),
      switchMap((user) =>
        this.tenantContext.bootstrap().pipe(map(() => user)),
      ),
    );
  }

  private withCsrf<T>(request: () => Observable<T>): Observable<T> {
    return this.http.get<void>('/api/auth/csrf').pipe(switchMap(request));
  }

  private setAnonymous(): void {
    this.userState.set(null);
    this.state.set('anonymous');
  }
}
