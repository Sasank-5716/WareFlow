import { HttpInterceptorFn } from '@angular/common/http';
import { runtimeConfig } from '../config/runtime-config';

export const apiUrlInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith('/') || request.url.startsWith('//')) {
    return next(request);
  }

  const url = `${runtimeConfig.apiBaseUrl}${request.url}`;
  return next(request.clone({ url, withCredentials: true }));
};
