import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { catchError, forkJoin, map, of } from 'rxjs';
import { TenantContextService } from '../tenant/tenant-context.service';
import { EffectiveModule } from './navigation.models';

type AccessState = 'idle' | 'loading' | 'ready' | 'error';

interface ModuleResponse extends EffectiveModule {}
interface PermissionResponse {
  readonly permissions: readonly string[];
}

@Injectable({ providedIn: 'root' })
export class NavigationAccessService {
  private readonly http = inject(HttpClient);
  private readonly tenantContext = inject(TenantContextService);
  private readonly state = signal<AccessState>('idle');
  private readonly modules = signal<readonly EffectiveModule[]>([]);
  private readonly permissions = signal<ReadonlySet<string>>(new Set());

  readonly accessState = this.state.asReadonly();
  readonly effectiveModules = this.modules.asReadonly();
  readonly hasAccessData = computed(() => this.state() === 'ready');

  load(): void {
    if (!this.tenantContext.activeTenant()) {
      this.modules.set([]);
      this.permissions.set(new Set());
      this.state.set('idle');
      return;
    }

    this.state.set('loading');
    forkJoin({
      modules: this.http.get<readonly ModuleResponse[]>('/api/tenants/active/modules/'),
      permissions: this.http.get<PermissionResponse>('/api/tenants/active/permissions'),
    })
      .pipe(
        map(({ modules, permissions }) => ({ modules, permissions: new Set(permissions.permissions) })),
        catchError(() => of(null)),
      )
      .subscribe((access) => {
        if (!access) {
          this.state.set('error');
          return;
        }
        this.modules.set(access.modules);
        this.permissions.set(access.permissions);
        this.state.set('ready');
      });
  }

  canUse(moduleKey: string | undefined, permission: string | undefined): boolean {
    if (!moduleKey && !permission) {
      return true;
    }
    const module = moduleKey ? this.modules().find((item) => item.moduleKey === moduleKey) : undefined;
    return (!moduleKey || module?.effectiveAccess === true) &&
      (!permission || this.permissions().has(permission));
  }

  moduleReason(moduleKey: string | undefined): string | null {
    return moduleKey ? this.modules().find((item) => item.moduleKey === moduleKey)?.reason ?? null : null;
  }
}
