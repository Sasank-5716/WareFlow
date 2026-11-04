import { Component, input } from '@angular/core';

@Component({
  selector: 'app-dropdown',
  template: `
    <details>
      <summary>{{ label() }}</summary>
      <div class="menu" role="menu"><ng-content /></div>
    </details>
  `,
  styles: `
    details { position: relative; } summary { cursor: pointer; list-style: none; } summary::-webkit-details-marker { display: none; }
    .menu { background: var(--color-surface-elevated); border: 1px solid var(--color-border); border-radius: var(--radius-md); box-shadow: var(--shadow-lg); min-width: 12rem; padding: var(--space-2); position: absolute; right: 0; top: calc(100% + var(--space-2)); z-index: 5; }
  `,
})
export class DropdownComponent {
  readonly label = input.required<string>();
}
