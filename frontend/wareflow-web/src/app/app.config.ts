import {
  ApplicationConfig,
  ErrorHandler,
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
import { errorHandler } from './core/errors/error-handler';

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
      withInterceptors([apiUrlInterceptor, apiErrorInterceptor]),
      withXsrfConfiguration({
        cookieName: 'XSRF-TOKEN',
        headerName: 'X-XSRF-TOKEN',
      }),
    ),
  ],
};
