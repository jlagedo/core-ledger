import { Injectable } from '@angular/core';
import { NgbDateParserFormatter, NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';

/**
 * NgbDatepicker formatter for Brazilian date format (dd/MM/yyyy).
 *
 * Converts between string representation (dd/MM/yyyy) and NgbDateStruct
 * for use with ng-bootstrap datepicker components.
 *
 * @example
 * ```typescript
 * @Component({
 *   providers: [
 *     { provide: NgbDateParserFormatter, useClass: NgbDateBRParserFormatter }
 *   ]
 * })
 * export class MyFormComponent {
 *   // Datepicker will now use Brazilian format
 * }
 * ```
 *
 * @remarks
 * Intentionally NOT using `providedIn: 'root'` because this formatter
 * is provided at component level to allow different date formats in
 * different parts of the application. Provided via `providers` array
 * in components that use NgbDatepicker.
 *
 * @see {@link https://ng-bootstrap.github.io/#/components/datepicker/api#NgbDateParserFormatter}
 */
@Injectable()
export class NgbDateBRParserFormatter extends NgbDateParserFormatter {
  parse(value: string): NgbDateStruct | null {
    if (!value) {
      return null;
    }

    const parts = value.trim().split('/');
    if (parts.length === 3) {
      const day = parseInt(parts[0], 10);
      const month = parseInt(parts[1], 10);
      const year = parseInt(parts[2], 10);

      if (!isNaN(day) && !isNaN(month) && !isNaN(year)) {
        return { day, month, year };
      }
    }
    return null;
  }

  format(date: NgbDateStruct | null): string {
    if (!date) {
      return '';
    }

    const day = String(date.day).padStart(2, '0');
    const month = String(date.month).padStart(2, '0');
    const year = String(date.year);

    return `${day}/${month}/${year}`;
  }
}
