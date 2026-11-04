import {
  HttpErrorResponse,
  HttpInterceptorFn,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { ApiError } from '../models/api-error.model';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && auth.isAuthenticated) {
        auth.markSessionExpired();
        void router.navigate(['/auth/session-expired']);
      }
      const apiError: ApiError = {
        status: error.status,
        code: readErrorCode(error),
        message: readErrorMessage(error),
        traceId: readTraceId(error),
        validationErrors: readValidationErrors(error),
      };
      return throwError(() => apiError);
    }),
  );
};

function readErrorCode(error: HttpErrorResponse): string {
  const body = asRecord(error.error);
  return typeof body?.['code'] === 'string'
    ? body['code']
    : typeof body?.['error'] === 'string'
      ? body['error']
      : `http_${error.status}`;
}

function readErrorMessage(error: HttpErrorResponse): string {
  const body = asRecord(error.error);
  return typeof body?.['message'] === 'string'
    ? body['message']
    : error.status === 0
      ? 'The service is unavailable. Check your connection.'
      : 'The request could not be completed.';
}

function readTraceId(error: HttpErrorResponse): string | undefined {
  const body = asRecord(error.error);
  return typeof body?.['traceId'] === 'string' ? body['traceId'] : undefined;
}

function readValidationErrors(
  error: HttpErrorResponse,
): Readonly<Record<string, readonly string[]>> | undefined {
  const body = asRecord(error.error)?.['errors'];
  return isValidationErrors(body) ? body : undefined;
}

function asRecord(value: unknown): Record<string, unknown> | undefined {
  return typeof value === 'object' && value !== null
    ? (value as Record<string, unknown>)
    : undefined;
}

function isValidationErrors(
  value: unknown,
): value is Readonly<Record<string, readonly string[]>> {
  return (
    typeof value === 'object' &&
    value !== null &&
    Object.values(value).every(
      (messages) =>
        Array.isArray(messages) &&
        messages.every((message) => typeof message === 'string'),
    )
  );
}
