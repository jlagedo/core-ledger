import { Directive, ElementRef, forwardRef, inject, input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

/**
 * Directive that applies Brazilian currency mask to input fields.
 * Implements ControlValueAccessor for seamless integration with Reactive Forms.
 *
 * Usage: <input appCurrencyMask formControlName="valor" />
 *
 * Features:
 * - Auto-formats in real-time as user types (1.234,56)
 * - Allows only numeric input and decimal separator
 * - Handles paste operations
 * - Maintains cursor position correctly
 * - Stores raw numeric value (number) in form control
 * - Displays formatted value in input
 * - Configurable decimal places (default: 2)
 */
@Directive({
  selector: '[appCurrencyMask]',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CurrencyMaskDirective),
      multi: true,
    },
  ],
  host: {
    '(input)': 'onInput($event)',
    '(paste)': 'onPaste()',
    '(blur)': 'onBlur()',
    '(keydown)': 'onKeyDown($event)',
    '(focus)': 'onFocus()',
  },
})
export class CurrencyMaskDirective implements ControlValueAccessor {
  private readonly el = inject(ElementRef<HTMLInputElement>);

  /** Number of decimal places (default: 2) */
  decimalPlaces = input<number>(2);

  private onChange: (value: number | null) => void = () => {
    /* noop */
  };
  private onTouched: () => void = () => {
    /* noop */
  };

  /**
   * Write a new value to the input (called by form control)
   * Receives raw numeric value, displays formatted
   */
  writeValue(value: number | null): void {
    const input = this.el.nativeElement;
    if (value !== null && value !== undefined) {
      input.value = this.formatNumber(value);
    } else {
      input.value = '';
    }
  }

  registerOnChange(fn: (value: number | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.el.nativeElement.disabled = isDisabled;
  }

  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.formatAndEmit(input);
  }

  onPaste(): void {
    // Let paste happen, then format on next tick
    setTimeout(() => {
      const input = this.el.nativeElement;
      this.formatAndEmit(input);
      this.onTouched();
    }, 0);
  }

  onBlur(): void {
    this.onTouched();
    // Ensure proper formatting on blur
    const input = this.el.nativeElement;
    const numericValue = this.parseToNumber(input.value);
    if (numericValue !== null) {
      input.value = this.formatNumber(numericValue);
    }
  }

  onFocus(): void {
    // Select all on focus for easy editing
    const input = this.el.nativeElement;
    setTimeout(() => input.select(), 0);
  }

  onKeyDown(event: KeyboardEvent): void {
    const allowedKeys = [
      'Backspace',
      'Delete',
      'Tab',
      'ArrowLeft',
      'ArrowRight',
      'Home',
      'End',
    ];

    if (allowedKeys.includes(event.key)) {
      return;
    }

    // Allow Ctrl/Cmd + A, C, V, X (select all, copy, paste, cut)
    if (event.ctrlKey || event.metaKey) {
      return;
    }

    // Allow digits
    if (/^[0-9]$/.test(event.key)) {
      return;
    }

    // Allow comma as decimal separator (Brazilian format)
    if (event.key === ',') {
      const input = this.el.nativeElement;
      // Only allow one comma
      if (input.value.includes(',')) {
        event.preventDefault();
      }
      return;
    }

    // Block all other keys
    event.preventDefault();
  }

  /**
   * Format the input value and emit to form control
   */
  private formatAndEmit(input: HTMLInputElement): void {
    const cursorPosition = input.selectionStart ?? 0;
    const oldValue = input.value;

    // Parse to number and format
    const numericValue = this.parseToNumber(oldValue);

    if (numericValue === null && oldValue.trim() === '') {
      input.value = '';
      this.onChange(null);
      return;
    }

    // Format for display (partial formatting while typing)
    const formattedValue = this.formatPartial(oldValue);
    input.value = formattedValue;

    // Calculate and restore cursor position
    const newPosition = this.calculateCursorPosition(
      oldValue,
      formattedValue,
      cursorPosition
    );
    requestAnimationFrame(() => {
      input.setSelectionRange(newPosition, newPosition);
    });

    // Emit numeric value to form control
    this.onChange(numericValue);
  }

  /**
   * Parse string to number (handles Brazilian format)
   */
  private parseToNumber(value: string): number | null {
    if (!value || value.trim() === '') {
      return null;
    }

    // Remove thousand separators (.) and replace decimal separator (,) with (.)
    const normalized = value.replace(/\./g, '').replace(',', '.');

    const num = parseFloat(normalized);
    return isNaN(num) ? null : num;
  }

  /**
   * Format number to Brazilian currency string
   */
  private formatNumber(value: number): string {
    const decimals = this.decimalPlaces();
    return value.toLocaleString('pt-BR', {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals,
    });
  }

  /**
   * Format partial input (while typing)
   * Only adds thousand separators, keeps user's decimal input
   */
  private formatPartial(value: string): string {
    // Remove all non-numeric except comma
    let cleaned = value.replace(/[^\d,]/g, '');

    // Handle multiple commas - keep only first
    const commaIndex = cleaned.indexOf(',');
    if (commaIndex !== -1) {
      const beforeComma = cleaned.slice(0, commaIndex).replace(/,/g, '');
      const afterComma = cleaned.slice(commaIndex + 1).replace(/,/g, '');
      cleaned = beforeComma + ',' + afterComma;
    }

    // Split into integer and decimal parts
    const parts = cleaned.split(',');
    let integerPart = parts[0] || '';
    const decimalPart = parts[1];

    // Remove leading zeros (except if it's just "0")
    if (integerPart.length > 1) {
      integerPart = integerPart.replace(/^0+/, '') || '0';
    }

    // Add thousand separators to integer part
    integerPart = this.addThousandSeparators(integerPart);

    // Limit decimal places
    const decimals = this.decimalPlaces();
    const limitedDecimal =
      decimalPart !== undefined ? decimalPart.slice(0, decimals) : undefined;

    // Reconstruct
    if (limitedDecimal !== undefined) {
      return integerPart + ',' + limitedDecimal;
    }
    return integerPart;
  }

  /**
   * Add thousand separators (.)
   */
  private addThousandSeparators(value: string): string {
    return value.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  }

  /**
   * Calculate the new cursor position after formatting
   */
  private calculateCursorPosition(
    oldValue: string,
    newValue: string,
    cursorPosition: number
  ): number {
    // Count digits before cursor in old value
    let digitCount = 0;
    for (let i = 0; i < cursorPosition && i < oldValue.length; i++) {
      if (/[\d,]/.test(oldValue[i])) {
        digitCount++;
      }
    }

    // Find equivalent position in new value
    let newDigitCount = 0;
    for (let i = 0; i < newValue.length; i++) {
      if (newDigitCount >= digitCount) {
        return i;
      }
      if (/[\d,]/.test(newValue[i])) {
        newDigitCount++;
      }
    }

    return newValue.length;
  }
}
