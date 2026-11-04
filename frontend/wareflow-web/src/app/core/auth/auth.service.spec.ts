import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { AuthService } from './auth.service';
import { TenantContextService } from '../tenant/tenant-context.service';
import { authGuard } from './auth.guard';
import { provideRouter, Router } from '@angular/router';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        TenantContextService,
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('restores an authenticated session and active tenant on refresh', async () => {
    const bootstrap = service.bootstrap();
    http.expectOne('/api/auth/csrf').flush(null);
    await Promise.resolve();
    http.expectOne('/api/auth/me').flush({
      id: 'user-1',
      email: 'person@example.com',
      displayName: 'Person',
      isEmailConfirmed: true,
    });
    await Promise.resolve();
    http.expectOne('/api/tenants').flush([
      {
        id: 'tenant-1',
        name: 'Acme',
        slug: 'acme',
        role: 'Owner',
        membershipStatus: 'Active',
      },
    ]);
    await Promise.resolve();
    http.expectOne('/api/tenants/active').flush({
      id: 'tenant-1',
      name: 'Acme',
      slug: 'acme',
      role: 'Owner',
      membershipStatus: 'Active',
    });
    await bootstrap;

    expect(service.isAuthenticated).toBe(true);
    expect(service.user()?.email).toBe('person@example.com');
    expect(TestBed.inject(TenantContextService).activeTenant()?.id).toBe('tenant-1');
  });

  it('enters anonymous state after an unauthorized refresh', async () => {
    const bootstrap = service.bootstrap();
    http.expectOne('/api/auth/csrf').flush(null);
    await Promise.resolve();
    http.expectOne('/api/auth/me').flush({}, { status: 401, statusText: 'Unauthorized' });
    await bootstrap;

    expect(service.authState()).toBe('anonymous');
    expect(service.user()).toBeNull();
  });

  it('logs in and then restores the server session', async () => {
    const login = firstValueFrom(
      service.login({
        email: 'person@example.com',
        password: 'password123',
        rememberMe: true,
      }),
    );
    http.expectOne('/api/auth/csrf').flush(null);
    await Promise.resolve();
    http.expectOne('/api/auth/login').flush(null);
    await Promise.resolve();
    http.expectOne('/api/auth/me').flush({
      id: 'user-1',
      email: 'person@example.com',
      displayName: null,
      isEmailConfirmed: true,
    });
    await Promise.resolve();
    http.expectOne('/api/tenants').flush([]);
    await login;

    expect(service.isAuthenticated).toBe(true);
  });

  it('clears the session and tenant context on logout', async () => {
    service.markSessionExpired();
    const logout = firstValueFrom(service.logout());
    http.expectOne('/api/auth/csrf').flush(null);
    await Promise.resolve();
    http.expectOne('/api/auth/logout').flush(null);
    await logout;

    expect(service.authState()).toBe('anonymous');
    expect(TestBed.inject(TenantContextService).activeTenant()).toBeNull();
  });

  it('redirects anonymous users to login from protected routes', () => {
    service.markSessionExpired();
    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as never, {} as never),
    );

    expect((result as ReturnType<Router['createUrlTree']>).toString()).toBe(
      '/auth/login',
    );
  });
});
