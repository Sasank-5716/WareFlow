import { HttpInterceptorFn } from '@angular/common/http';
import { runtimeConfig } from '../config/runtime-config';

const unsafeMethods = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);

export const csrfInterceptor: HttpInterceptorFn = (request, next) => {
  if (!unsafeMethods.has(request.method) || request.headers.has('X-XSRF-TOKEN')) {
    return next(request);
  }

  const token = readCookie('XSRF-TOKEN');
  if (!token) {
    return next(request);
  }

  return next(
    request.clone({
      headers: request.headers.set('X-XSRF-TOKEN', decodeURIComponent(token)),
      withCredentials: request.withCredentials || isCrossOriginApi(),
    }),
  );
};

function readCookie(name: string): string | undefined {
  const prefix = `${name}=`;
  return document.cookie
    .split(';')
    .map((cookie) => cookie.trim())
    .find((cookie) => cookie.startsWith(prefix))
    ?.slice(prefix.length);
}

function isCrossOriginApi(): boolean {
  if (!runtimeConfig.apiBaseUrl) return false;
  return (
    new URL(runtimeConfig.apiBaseUrl, window.location.origin).origin !==
    window.location.origin
  );
}
