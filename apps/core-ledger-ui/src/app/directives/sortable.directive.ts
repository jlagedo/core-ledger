import { Directive, input, output, signal } from '@angular/core';

export type SortDirection = 'asc' | 'desc' | '';

const rotate: { [key: string]: SortDirection } = {
  asc: 'desc',
  desc: '',
  '': 'asc',
};

export interface SortEvent {
  column: string;
  direction: SortDirection;
}

@Directive({
  selector: 'th[sortable], [sortable]',
  host: {
    '[class.asc]': 'direction() === "asc"',
    '[class.desc]': 'direction() === "desc"',
    '(click)': 'onRotate()',
    '(keydown.enter)': 'onRotate()',
    '(keydown.space)': 'onKeyboardActivate($event)',
    '[style.cursor]': '"pointer"',
    '[attr.tabindex]': '"0"',
    '[attr.role]': '"columnheader"',
    '[attr.aria-sort]': 'ariaSort()',
  },
})
export class SortableDirective {
  // Signal-based inputs (Angular 21 best practice)
  readonly sortable = input<string>('');
  readonly direction = signal<SortDirection>('');

  // Event output
  readonly sort = output<SortEvent>();

  /**
   * Computed aria-sort value for accessibility
   */
  ariaSort(): 'ascending' | 'descending' | 'none' {
    const dir = this.direction();
    if (dir === 'asc') return 'ascending';
    if (dir === 'desc') return 'descending';
    return 'none';
  }

  /**
   * Rotate sort direction on click or Enter key
   */
  onRotate(): void {
    const newDirection = rotate[this.direction()];
    this.direction.set(newDirection);
    this.sort.emit({ column: this.sortable(), direction: newDirection });
  }

  /**
   * Handle Space key activation (prevent scroll)
   */
  onKeyboardActivate(event: Event): void {
    event.preventDefault();
    this.onRotate();
  }
}
