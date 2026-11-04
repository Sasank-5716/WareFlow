import { ErrorHandler, inject, Injectable } from '@angular/core';
import { NotificationService } from '../services/notification.service';

@Injectable()
class AppErrorHandler implements ErrorHandler {
  private readonly notifications = inject(NotificationService);

  handleError(error: unknown): void {
    console.error(error);
    this.notifications.show({
      tone: 'error',
      message: 'Something went wrong. Please try again.',
    });
  }
}

export function errorHandler(): ErrorHandler {
  return new AppErrorHandler();
}
