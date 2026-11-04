import { Component, input } from '@angular/core';

@Component({
  selector: 'app-form-field',
  template: `
    <label [attr.for]="id()">{{ label() }}</label>
    <ng-content />
    @if (hint()) { <small [id]="id() + '-hint'">{{ hint() }}</small> }
  `,
  styles: `
    :host { display: grid; gap: var(--space-2); }
    label { color: var(--color-text); font-size: var(--font-size-sm); font-weight: 650; }
    small { color: var(--color-text-muted); font-size: var(--font-size-sm); }
    ::ng-deep input, ::ng-deep select, ::ng-deep textarea { background: var(--color-surface); border: 1px solid var(--color-border-strong); border-radius: var(--radius-sm); color: var(--color-text); font: inherit; min-height: 2.6rem; padding: 0 var(--space-3); }
    ::ng-deep input:focus-visible, ::ng-deep select:focus-visible, ::ng-deep textarea:focus-visible { border-color: var(--color-primary); outline: 3px solid var(--color-focus); outline-offset: 1px; }
  `,
})
export class FormFieldComponent {
  readonly id = input.required<string>();
  readonly label = input.required<string>();
  readonly hint = input('');
}
