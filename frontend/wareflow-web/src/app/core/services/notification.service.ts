import { Injectable, signal } from '@angular/core';

export type NotificationTone = 'info' | 'success' | 'warning' | 'error';

export interface Notification {
  readonly id: number;
  readonly tone: NotificationTone;
  readonly message: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private nextId = 0;
  readonly notifications = signal<readonly Notification[]>([]);

  show(notification: Omit<Notification, 'id'>): void {
    const item = { ...notification, id: this.nextId++ };
    this.notifications.update((items) => [...items, item]);
    window.setTimeout(() => this.dismiss(item.id), 5000);
  }

  dismiss(id: number): void {
    this.notifications.update((items) => items.filter((item) => item.id !== id));
  }
}
