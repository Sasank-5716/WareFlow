import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of } from 'rxjs';

export interface AuthenticatedUser {
  readonly id: string;
  readonly email: string;
  readonly displayName?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly userState = signal<AuthenticatedUser | null>(null);
  readonly user = this.userState.asReadonly();

  constructor(private readonly http: HttpClient) {}

  loadSession(): void {
    this.http
      .get<AuthenticatedUser>('/api/auth/me')
      .pipe(catchError(() => of(null)))
      .subscribe((user) => this.userState.set(user));
  }

  get isAuthenticated(): boolean {
    return this.user() !== null;
  }
}
