import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TenantContextService } from '../tenant/tenant-context.service';

export const tenantHeaderInterceptor: HttpInterceptorFn = (request, next) => {
  const tenantId = inject(TenantContextService).activeTenant()?.id;
  if (!tenantId || !isTenantScopedRequest(request.url)) {
    return next(request);
  }

  return next(request.clone({ setHeaders: { 'X-Tenant-Id': tenantId } }));
};

function isTenantScopedRequest(url: string): boolean {
  const path = new URL(url, window.location.origin).pathname;
  if (!path.startsWith('/api/') || path.startsWith('/api/auth/')) {
    return false;
  }

  return !(
    path === '/api/tenants' ||
    path === '/api/tenants/' ||
    path === '/api/tenants/active' ||
    path.endsWith('/select') ||
    path === '/api/tenants/invitations/accept' ||
    path === '/api/tenants/invitations/reject'
  );
}
