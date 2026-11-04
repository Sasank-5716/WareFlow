import { Component, input, output } from '@angular/core';
import { ButtonComponent } from '../button/button.component';
import { DialogComponent } from '../dialog/dialog.component';

@Component({
  imports: [ButtonComponent, DialogComponent],
  selector: 'app-confirmation',
  template: `
    <app-dialog [open]="open()" [title]="title()" (closed)="cancelled.emit()">
      <p>{{ message() }}</p>
      <footer>
        <button appButton variant="secondary" type="button" (click)="cancelled.emit()">Cancel</button>
        <button appButton type="button" (click)="confirmed.emit()">Confirm</button>
      </footer>
    </app-dialog>
  `,
  styles: `footer { display: flex; gap: var(--space-3); justify-content: end; margin-top: var(--space-6); }`,
})
export class ConfirmationComponent {
  readonly open = input(false);
  readonly title = input('Confirm action');
  readonly message = input('Are you sure you want to continue?');
  readonly confirmed = output<void>();
  readonly cancelled = output<void>();
}
