import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { TenantContextService } from './tenant-context.service';
import { TenantScopedStateService } from './tenant-scoped-state.service';

describe('TenantContextService', () => {
  let service: TenantContextService;
  let http: HttpTestingController;
  let scopedState: TenantScopedStateService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        TenantContextService,
        TenantScopedStateService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(TenantContextService);
    http = TestBed.inject(HttpTestingController);
    scopedState = TestBed.inject(TenantScopedStateService);
  });

  afterEach(() => http.verify());

  it('switches the active tenant and invalidates tenant-scoped state', async () => {
    const switching = firstValueFrom(service.selectTenant('tenant-2'));
    http.expectOne('/api/tenants/tenant-2/select').flush({
      id: 'tenant-2',
      name: 'Beta',
      slug: 'beta',
      role: 'Member',
      membershipStatus: 'Active',
    });
    await switching;

    expect(service.activeTenant()?.id).toBe('tenant-2');
    expect(scopedState.invalidationVersion()).toBe(1);
  });

  it('keeps the active tenant when selection fails', async () => {
    const bootstrap = firstValueFrom(service.bootstrap());
    http.expectOne('/api/tenants').flush([
      {
        id: 'tenant-1',
        name: 'Acme',
        slug: 'acme',
        role: 'Owner',
        membershipStatus: 'Active',
      },
    ]);
    await bootstrap;
    const active = service.restoreActiveTenant();
    http.expectOne('/api/tenants/active').flush({
      id: 'tenant-1',
      name: 'Acme',
      slug: 'acme',
      role: 'Owner',
      membershipStatus: 'Active',
    });
    await active;

    const switching = firstValueFrom(service.selectTenant('missing'));
    http
      .expectOne('/api/tenants/missing/select')
      .flush(null, { status: 404, statusText: 'Not Found' });
    await expect(switching).rejects.toBeDefined();

    expect(service.activeTenant()?.id).toBe('tenant-1');
    expect(scopedState.invalidationVersion()).toBe(0);
  });
});
