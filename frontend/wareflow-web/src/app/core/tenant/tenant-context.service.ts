import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { catchError, firstValueFrom, map, Observable, of, tap } from 'rxjs';
import { TenantScopedStateService } from './tenant-scoped-state.service';

export interface TenantSummary {
  readonly id: string;
  readonly name: string;
  readonly slug: string;
  readonly role: string;
  readonly membershipStatus: string;
}

export type ActiveTenantContext = TenantSummary;
export type TenantState = 'loading' | 'ready' | 'empty' | 'error';

@Injectable({ providedIn: 'root' })
export class TenantContextService {
  private readonly tenantsState = signal<readonly TenantSummary[]>([]);
  private readonly activeTenantState = signal<ActiveTenantContext | null>(null);
  private readonly state = signal<TenantState>('loading');

  readonly tenants = this.tenantsState.asReadonly();
  readonly activeTenant = this.activeTenantState.asReadonly();
  readonly tenantState = this.state.asReadonly();

  constructor(
    private readonly http: HttpClient,
    private readonly scopedState: TenantScopedStateService,
  ) {}

  bootstrap(): Observable<void> {
    return this.http.get<readonly TenantSummary[]>('/api/tenants').pipe(
      tap((tenants) => this.tenantsState.set(tenants)),
      tap(() => this.state.set(this.tenants().length > 0 ? 'ready' : 'empty')),
      tap(() => {
        if (this.tenants().length === 0) {
          this.activeTenantState.set(null);
        }
      }),
      map(() => undefined),
      catchError(() => {
        this.state.set('error');
        return of(undefined);
      }),
    );
  }

  async restoreActiveTenant(): Promise<void> {
    try {
      const active = await firstValueFrom(
        this.http.get<ActiveTenantContext>('/api/tenants/active'),
      );
      this.activeTenantState.set(active);
    } catch {
      this.activeTenantState.set(null);
    }
  }

  selectTenant(tenantId: string): Observable<ActiveTenantContext> {
    return this.http
      .post<ActiveTenantContext>(`/api/tenants/${tenantId}/select`, null)
      .pipe(
        tap((active) => {
          this.activeTenantState.set(active);
          this.scopedState.invalidate();
        }),
      );
  }

  clear(): void {
    this.tenantsState.set([]);
    this.activeTenantState.set(null);
    this.state.set('empty');
    this.scopedState.invalidate();
  }
}
