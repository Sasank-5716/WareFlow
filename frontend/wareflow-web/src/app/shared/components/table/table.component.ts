import { Component } from '@angular/core';

@Component({
  selector: 'app-table',
  template: `<div class="table-wrap"><table><ng-content /></table></div>`,
  styles: `
    .table-wrap { overflow-x: auto; }
    table { border-collapse: collapse; min-width: 100%; }
    ::ng-deep th, ::ng-deep td { border-bottom: 1px solid var(--color-border); padding: var(--space-3); text-align: start; }
    ::ng-deep th { color: var(--color-text-muted); font-size: var(--font-size-sm); font-weight: 700; }
  `,
})
export class TableComponent {}
