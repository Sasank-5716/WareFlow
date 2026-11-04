import {
  ApplicationConfig,
  ErrorHandler,
  inject,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import {
  provideHttpClient,
  withInterceptors,
  withXsrfConfiguration,
} from '@angular/common/http';
import {
  provideRouter,
  withComponentInputBinding,
  withNavigationErrorHandler,
  withViewTransitions,
} from '@angular/router';
import { routes } from './app.routes';
import { apiErrorInterceptor } from './core/http/api-error.interceptor';
import { apiUrlInterceptor } from './core/http/api-url.interceptor';
import { csrfInterceptor } from './core/http/csrf.interceptor';
import { tenantHeaderInterceptor } from './core/http/tenant-header.interceptor';
import { errorHandler } from './core/errors/error-handler';
import { provideAppInitializer } from '@angular/core';
import { AuthService } from './core/auth/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    { provide: ErrorHandler, useFactory: errorHandler },
    provideRouter(
      routes,
      withComponentInputBinding(),
      withViewTransitions(),
      withNavigationErrorHandler((error) => {
        console.error('Navigation failed', error);
      }),
    ),
    provideHttpClient(
      withInterceptors([
        apiUrlInterceptor,
        csrfInterceptor,
        tenantHeaderInterceptor,
        apiErrorInterceptor,
      ]),
      withXsrfConfiguration({
        cookieName: 'XSRF-TOKEN',
        headerName: 'X-XSRF-TOKEN',
      }),
    ),
    provideAppInitializer(() => inject(AuthService).bootstrap()),
  ],
};
